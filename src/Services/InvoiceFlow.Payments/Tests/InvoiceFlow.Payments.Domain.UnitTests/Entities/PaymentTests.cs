using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Domain.UnitTests.Entities;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Created = DateTimeOffset.Parse("2025-01-01T10:00:00Z");
    private static Payment NewPayment(decimal amount = 100m) => Payment.Create(
        Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), CurrencyCode.Create("RON"),
        Money.Create(amount, CurrencyCode.Create("RON")), PaymentMethod.Card,
        PaymentReference.CreateOptional(" ref "), PaymentNotes.CreateOptional(" notes "), Created);

    [Fact]
    public void Create_sets_draft_state_and_utc_timestamp()
    {
        var payment = NewPayment();
        payment.Status.Should().Be(PaymentLifecycleStatus.Draft);
        payment.Number.Should().BeNull();
        payment.AllocatedAmount.Amount.Should().Be(0m);
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.CreatedAtUtc.Should().Be(Created);
    }

    [Fact]
    public void Create_rejects_invalid_identity_amount_currency_and_method()
    {
        var tenant = Guid.CreateVersion7(); var id = Guid.CreateVersion7(); var customer = Guid.CreateVersion7();
        Action emptyTenant = () => Payment.Create(Guid.Empty, id, customer, CurrencyCode.Create("RON"), Money.Create(1m, CurrencyCode.Create("RON")), PaymentMethod.Card, null, null, Created);
        emptyTenant.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
        Action emptyCustomer = () => Payment.Create(tenant, id, Guid.Empty, CurrencyCode.Create("RON"), Money.Create(1m, CurrencyCode.Create("RON")), PaymentMethod.Card, null, null, Created);
        emptyCustomer.Should().Throw<DomainException>().WithDomainError(DomainErrors.CustomerIdRequired);
        Action zeroAmount = () => Payment.Create(tenant, id, customer, CurrencyCode.Create("RON"), Money.Zero(CurrencyCode.Create("RON")), PaymentMethod.Card, null, null, Created);
        zeroAmount.Should().Throw<DomainException>().WithDomainError(DomainErrors.AmountInvalid);
        Action currencyMismatch = () => Payment.Create(tenant, id, customer, CurrencyCode.Create("RON"), Money.Create(1m, CurrencyCode.Create("EUR")), PaymentMethod.Card, null, null, Created);
        currencyMismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
        Action invalidMethod = () => Payment.Create(tenant, id, customer, CurrencyCode.Create("RON"), Money.Create(1m, CurrencyCode.Create("RON")), (PaymentMethod)99, null, null, Created);
        invalidMethod.Should().Throw<DomainException>().WithDomainError(DomainErrors.MethodInvalid);
    }

    [Fact]
    public void Add_and_remove_allocation_updates_balances_and_timestamp()
    {
        var payment = NewPayment(); var at = Created.AddHours(1);
        var allocationId = Guid.CreateVersion7(); var invoiceId = Guid.CreateVersion7();
        payment.AddAllocation(allocationId, invoiceId, InvoiceNumber.Create("inv-1"), Money.Create(40m, CurrencyCode.Create("RON")), at);
        payment.AllocatedAmount.Amount.Should().Be(40m);
        payment.UnallocatedAmount.Amount.Should().Be(60m);
        payment.HasAllocationForInvoice(invoiceId).Should().BeTrue();
        payment.UpdatedAtUtc.Should().Be(at);

        payment.RemoveAllocation(allocationId, at.AddMinutes(1));
        payment.Allocations.Should().BeEmpty();
        payment.UnallocatedAmount.Amount.Should().Be(100m);
        payment.HasAllocationForInvoice(invoiceId).Should().BeFalse();
    }

    [Fact]
    public void AddAllocation_rejects_duplicate_invoice_and_overallocation()
    {
        var payment = NewPayment(); var invoiceId = Guid.CreateVersion7(); var at = Created.AddHours(1);
        payment.AddAllocation(Guid.CreateVersion7(), invoiceId, InvoiceNumber.Create("INV-1"), Money.Create(40m, CurrencyCode.Create("RON")), at);
        Action duplicate = () => payment.AddAllocation(Guid.CreateVersion7(), invoiceId, InvoiceNumber.Create("INV-1"), Money.Create(10m, CurrencyCode.Create("RON")), at.AddMinutes(1));
        duplicate.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceIdInvalid);
        Action over = () => payment.AddAllocation(Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceNumber.Create("INV-2"), Money.Create(70m, CurrencyCode.Create("RON")), at.AddMinutes(1));
        over.Should().Throw<DomainException>().WithDomainError(DomainErrors.AmountExceedsUnallocated);
    }

    [Fact]
    public void ChangeAmount_cannot_go_below_allocated_amount()
    {
        var payment = NewPayment();
        payment.AddAllocation(Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceNumber.Create("INV-1"), Money.Create(40m, CurrencyCode.Create("RON")), Created.AddHours(1));
        Action act = () => payment.ChangeAmount(Money.Create(30m, CurrencyCode.Create("RON")), Created.AddHours(2));
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.PaymentAmountBelowAllocated);
    }

    [Fact]
    public void Post_requires_complete_allocation_then_sets_posted_fields()
    {
        var payment = NewPayment(); var at = Created.AddHours(1); var date = new DateOnly(2025, 1, 2);
        Action noAllocations = () => payment.Post(PaymentNumber.Create("PAY-1"), date, at);
        noAllocations.Should().Throw<DomainException>().WithDomainError(DomainErrors.NoAllocations);
        payment.AddAllocation(Guid.CreateVersion7(), Guid.CreateVersion7(), InvoiceNumber.Create("INV-1"), Money.Create(100m, CurrencyCode.Create("RON")), at);
        payment.Post(PaymentNumber.Create(" pay-2025-000001 "), date, at.AddMinutes(1));
        payment.Status.Should().Be(PaymentLifecycleStatus.Posted);
        payment.Number!.Value.Should().Be("PAY-2025-000001");
        payment.PaymentDate.Should().Be(date);
        payment.IsFullyAllocated().Should().BeTrue();
        Action modify = () => payment.ChangeNotes(null, at.AddMinutes(2));
        modify.Should().Throw<DomainException>().WithDomainError(DomainErrors.CanNotBeModified);
    }
}
