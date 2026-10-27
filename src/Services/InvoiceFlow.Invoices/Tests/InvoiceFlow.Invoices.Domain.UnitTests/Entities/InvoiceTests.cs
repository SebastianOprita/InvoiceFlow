using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

public sealed class InvoiceTests
{
    [Fact]
    public void Create_InitializesDraftFromCustomerSnapshot()
    {
        var customer = InvoiceEntityTestData.Customer();
        var invoice = InvoiceEntityTestData.Draft(customer);

        invoice.TenantId.Should().Be(customer.CustomerTenantId);
        invoice.CustomerId.Should().Be(customer.CustomerId);
        invoice.CustomerCode.Should().Be(customer.CustomerCode);
        invoice.CurrencyCode.Should().Be(customer.CurrencyCode);
        invoice.PaymentTermDays.Should().Be(customer.PaymentTermDays);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Number.Should().BeNull();
        invoice.Notes!.Value.Should().Be("Notes");
        invoice.GrossAmount.Amount.Should().Be(0m);
        invoice.RemainingAmount.Amount.Should().Be(0m);
        invoice.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        invoice.CreatedAtUtc.Should().Be(InvoiceEntityTestData.CreatedAt);
    }

    [Fact]
    public void Create_RejectsInvalidTenantId()
    {
        Action act = () => Invoice.Create(Guid.Empty, Guid.CreateVersion7(), InvoiceEntityTestData.Customer(), null, InvoiceEntityTestData.CreatedAt);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsInvalidId()
    {
        Action act = () => Invoice.Create(InvoiceEntityTestData.TenantId, Guid.Empty, InvoiceEntityTestData.Customer(), null, InvoiceEntityTestData.CreatedAt);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Create_RejectsMissingCreatedDate()
    {
        Action act = () => Invoice.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), InvoiceEntityTestData.Customer(), null, default);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CreatedAtUtcRequired);
    }

    [Fact]
    public void Create_RejectsCustomerFromAnotherTenantOrInactiveCustomer()
    {
        Action mismatch = () => Invoice.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), InvoiceEntityTestData.Customer(Guid.CreateVersion7()), null, InvoiceEntityTestData.CreatedAt);
        Action inactive = () => Invoice.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), InvoiceEntityTestData.Customer(active: false), null, InvoiceEntityTestData.CreatedAt);

        mismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantMismatch);
        inactive.Should().Throw<DomainException>().WithDomainError(DomainErrors.CustomerInactive);
    }

    [Fact]
    public void ChangeHeader_UpdatesCustomerTermsCurrencyAndNotes()
    {
        var invoice = InvoiceEntityTestData.Draft();
        var updatedAt = InvoiceEntityTestData.CreatedAt.AddHours(1);
        var newCustomer = InvoiceEntityTestData.Customer(paymentTerms: 45);
        invoice.ChangeCustomer(newCustomer, updatedAt);
        invoice.ChangePaymentTerms(PaymentTermDays.Create(60), updatedAt);
        invoice.ChangeCurrency(CurrencyCode.Create("EUR"), updatedAt);
        invoice.ChangeNotes(null, updatedAt);

        invoice.CustomerId.Should().Be(newCustomer.CustomerId);
        invoice.PaymentTermDays.Value.Should().Be(60);
        invoice.CurrencyCode.Value.Should().Be("EUR");
        invoice.GrossAmount.CurrencyCode.Value.Should().Be("EUR");
        invoice.Notes.Should().BeNull();
        invoice.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeHeader_RejectsWrongOrInactiveCustomer()
    {
        var invoice = InvoiceEntityTestData.Draft();
        Action mismatch = () => invoice.ChangeCustomer(InvoiceEntityTestData.Customer(Guid.CreateVersion7()), InvoiceEntityTestData.CreatedAt.AddHours(1));
        Action inactive = () => invoice.ChangeCustomer(InvoiceEntityTestData.Customer(active: false), InvoiceEntityTestData.CreatedAt.AddHours(1));
        mismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantMismatch);
        inactive.Should().Throw<DomainException>().WithDomainError(DomainErrors.CustomerInactive);
    }

    [Fact]
    public void ChangeCurrency_RejectsChangeAfterLinesExist()
    {
        var invoice = InvoiceEntityTestData.Draft();
        InvoiceEntityTestData.AddLine(invoice);
        Action act = () => invoice.ChangeCurrency(CurrencyCode.Create("EUR"), InvoiceEntityTestData.CreatedAt.AddHours(2));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyHasLines);
    }

    [Fact]
    public void AddProductLine_ValidatesProductAndCalculatesTotals()
    {
        var invoice = InvoiceEntityTestData.Draft();
        var product = InvoiceEntityTestData.Product(description: ProductDescription.Create("Consulting"));
        var line = invoice.AddProductLine(Guid.CreateVersion7(), product, Quantity.Create(2), Percentage.Create(10), InvoiceEntityTestData.CreatedAt.AddHours(1));

        line.Description.Value.Should().Be("Consulting");
        invoice.Lines.Should().ContainSingle();
        invoice.DiscountAmount.Amount.Should().Be(20m);
        invoice.NetAmount.Amount.Should().Be(180m);
        invoice.TaxAmount.Amount.Should().Be(34.20m);
        invoice.GrossAmount.Amount.Should().Be(214.20m);
    }

    [Fact]
    public void AddProductLine_RejectsMismatchedInactiveOrDuplicateProductLine()
    {
        var invoice = InvoiceEntityTestData.Draft();
        Action wrongTenant = () => invoice.AddProductLine(Guid.CreateVersion7(), InvoiceEntityTestData.Product(Guid.CreateVersion7()), Quantity.Create(1), Percentage.Zero, InvoiceEntityTestData.CreatedAt.AddHours(1));
        Action inactive = () => invoice.AddProductLine(Guid.CreateVersion7(), InvoiceEntityTestData.Product(active: false), Quantity.Create(1), Percentage.Zero, InvoiceEntityTestData.CreatedAt.AddHours(1));
        wrongTenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantMismatch);
        inactive.Should().Throw<DomainException>().WithDomainError(DomainErrors.ProductInactive);

        var lineId = Guid.CreateVersion7();
        InvoiceEntityTestData.AddLine(invoice, id: lineId);
        Action duplicate = () => InvoiceEntityTestData.AddLine(invoice, id: lineId);
        duplicate.Should().Throw<DomainException>().WithDomainError(DomainErrors.LineIdDuplicate);
    }

    [Fact]
    public void UpdateReplaceMoveAndRemoveLine_KeepPositionsAndTotalsConsistent()
    {
        var invoice = InvoiceEntityTestData.Draft();
        var first = InvoiceEntityTestData.AddLine(invoice);
        var second = InvoiceEntityTestData.AddLine(invoice);
        var changedAt = InvoiceEntityTestData.CreatedAt.AddHours(2);

        invoice.UpdateLine(first.Id, InvoiceLineDescription.Create("Updated"), Quantity.Create(2), UnitOfMeasure.Hour,
            Money.Create(50m, invoice.CurrencyCode), Percentage.Zero, Percentage.Create(10), changedAt);
        first.Description.Value.Should().Be("Updated");
        invoice.GrossAmount.Amount.Should().Be(229m);

        var product = InvoiceEntityTestData.Product(description: ProductDescription.Create("Replacement"), price: 25m);
        invoice.ReplaceLineProduct(second.Id, product, changedAt);
        second.Description.Value.Should().Be("Replacement");
        invoice.MoveLine(second.Id, 1, changedAt);
        invoice.Lines.Select(x => x.Id).Should().ContainInOrder(second.Id, first.Id);
        invoice.RemoveLine(second.Id, changedAt);
        invoice.Lines.Should().ContainSingle().Which.Id.Should().Be(first.Id);
        first.Position.Should().Be(1);
        invoice.GrossAmount.Amount.Should().Be(first.GrossAmount.Amount);
    }

    [Fact]
    public void LineOperations_RejectMissingIdsAndInvalidPositions()
    {
        var invoice = InvoiceEntityTestData.Draft();
        Action emptyLineId = () => invoice.AddManualLine(Guid.Empty, InvoiceLineDescription.Create("x"), Quantity.Create(1), UnitOfMeasure.Service, Money.Create(1, invoice.CurrencyCode), Percentage.Zero, Percentage.Zero, InvoiceEntityTestData.CreatedAt.AddHours(1));
        Action emptyLookupId = () => invoice.RemoveLine(Guid.Empty, InvoiceEntityTestData.CreatedAt.AddHours(1));
        Action missingLine = () => invoice.RemoveLine(Guid.CreateVersion7(), InvoiceEntityTestData.CreatedAt.AddHours(1));
        Action invalidPosition = () => invoice.MoveLine(Guid.CreateVersion7(), 1, InvoiceEntityTestData.CreatedAt.AddHours(1));
        emptyLineId.Should().Throw<DomainException>().WithDomainError(DomainErrors.LineIdInvalid);
        emptyLookupId.Should().Throw<DomainException>().WithDomainError(DomainErrors.LineIdInvalid);
        missingLine.Should().Throw<DomainException>().WithDomainError(DomainErrors.LineIdNotFound);
        invalidPosition.Should().Throw<DomainException>().WithDomainError(DomainErrors.PositionInvalid);
    }

    [Fact]
    public void Issue_SetsInvoiceNumberDatesAndDerivedStatus()
    {
        var invoice = InvoiceEntityTestData.Draft();
        InvoiceEntityTestData.AddLine(invoice);
        var issueDate = new DateOnly(2025, 1, 2);
        var issuedAt = InvoiceEntityTestData.CreatedAt.AddDays(1);
        invoice.Issue(InvoiceNumber.Create("inv-2025-1"), issueDate, issuedAt);

        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.Number!.Value.Should().Be("INV-2025-1");
        invoice.IssueDate.Should().Be(issueDate);
        invoice.DueDate.Should().Be(issueDate.AddDays(30));
        invoice.IssuedAtUtc.Should().Be(issuedAt);
        invoice.CanReceivePayment().Should().BeTrue();
        invoice.IsOverdue(issueDate.AddDays(31)).Should().BeTrue();
    }

    [Fact]
    public void Issue_RejectsDraftWithoutLinesOrMissingIssueTimestamp()
    {
        var empty = InvoiceEntityTestData.Draft();
        Action noLines = () => empty.Issue(InvoiceNumber.Create("INV-1"), new DateOnly(2025, 1, 1), InvoiceEntityTestData.CreatedAt);
        noLines.Should().Throw<DomainException>().WithDomainError(DomainErrors.NoLines);

        var invoice = InvoiceEntityTestData.Draft();
        InvoiceEntityTestData.AddLine(invoice);
        Action noTimestamp = () => invoice.Issue(InvoiceNumber.Create("INV-1"), new DateOnly(2025, 1, 1), default);
        noTimestamp.Should().Throw<DomainException>().WithDomainError(DomainErrors.IssuedAtUtcRequired);
    }

    [Fact]
    public void Cancel_RequiresIssuedInvoiceAndValidTimestamp()
    {
        var draft = InvoiceEntityTestData.Draft();
        Action beforeIssue = () => draft.Cancel(InvoiceEntityTestData.CreatedAt.AddDays(1));
        beforeIssue.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceNotIssued);

        var issued = InvoiceEntityTestData.Issued();
        Action noTimestamp = () => issued.Cancel(default);
        noTimestamp.Should().Throw<DomainException>().WithDomainError(DomainErrors.CancelledAtUtcRequired);

        var cancelledAt = InvoiceEntityTestData.CreatedAt.AddDays(2);
        issued.Cancel(cancelledAt);
        issued.Status.Should().Be(InvoiceStatus.Cancelled);
        issued.CancelledAtUtc.Should().Be(cancelledAt);
        issued.CanReceivePayment().Should().BeFalse();
    }

    [Fact]
    public void DraftOnlyOperations_RejectChangesAfterIssue()
    {
        var invoice = InvoiceEntityTestData.Issued();
        var updatedAt = InvoiceEntityTestData.CreatedAt.AddDays(2);
        Action notes = () => invoice.ChangeNotes(null, updatedAt);
        Action terms = () => invoice.ChangePaymentTerms(PaymentTermDays.Create(10), updatedAt);
        Action currency = () => invoice.ChangeCurrency(CurrencyCode.Create("EUR"), updatedAt);
        notes.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceNotDraft);
        terms.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceNotDraft);
        currency.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceNotDraft);
    }
}
