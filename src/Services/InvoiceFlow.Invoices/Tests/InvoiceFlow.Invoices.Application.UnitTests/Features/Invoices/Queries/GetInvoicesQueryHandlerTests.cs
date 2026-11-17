using FluentAssertions;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoicesQueryHandlerTests
{
    private readonly Mock<IInvoicesRepository> _invoices = new();
    private readonly GetInvoicesQueryHandler _sut;

    public GetInvoicesQueryHandlerTests()
    {
        _sut = new GetInvoicesQueryHandler(_invoices.Object);
    }

    [Fact]
    public async Task Handle_ReturnsInvoiceDtos_ForTenant()
    {
        var first = InvoiceTestData.DraftInvoice();
        first.Issue(InvoiceNumber.Create("INV-001"), new DateOnly(2025, 1, 1), new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var second = InvoiceTestData.DraftInvoice();
        var query = new GetInvoicesQuery(InvoiceTestData.TenantId);
        _invoices.Setup(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None))
            .ReturnsAsync([first, second]);
        _invoices.Setup(x => x.GetPaymentSummariesAsync(
                query.TenantId,
                It.IsAny<IReadOnlyCollection<Guid>>(),
                CancellationToken.None))
            .ReturnsAsync([CreatePartialSummary(first)]);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Id).Should().Contain([first.Id, second.Id]);
        result.Value.Should().OnlyContain(x => x.Lines.Count == 1);
        result.Value.Single(x => x.Id == first.Id).Lines.Single().Id.Should().Be(first.Lines.Single().Id);
        result.Value.Single(x => x.Id == second.Id).Lines.Single().Id.Should().Be(second.Lines.Single().Id);
        var firstDto = result.Value.Single(x => x.Id == first.Id);
        firstDto.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        firstDto.PaidAmount.Should().Be(25m);
        firstDto.RemainingAmount.Should().Be(first.GrossAmount.Amount - 25m);
        var secondDto = result.Value.Single(x => x.Id == second.Id);
        secondDto.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        secondDto.PaidAmount.Should().Be(0m);
        secondDto.RemainingAmount.Should().Be(second.GrossAmount.Amount);
        _invoices.Verify(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None), Times.Once);
        _invoices.Verify(x => x.GetPaymentSummariesAsync(
            query.TenantId,
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(first.Id)),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotLoadPaymentSummaries_WhenAllInvoicesAreDrafts()
    {
        var invoices = new List<Invoice> { InvoiceTestData.DraftInvoice(), InvoiceTestData.DraftInvoice() };
        var query = new GetInvoicesQuery(InvoiceTestData.TenantId);
        _invoices.Setup(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None))
            .ReturnsAsync(invoices);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        _invoices.Verify(x => x.GetPaymentSummariesAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenTenantHasNoInvoices()
    {
        var query = new GetInvoicesQuery(InvoiceTestData.TenantId);
        _invoices.Setup(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None))
            .ReturnsAsync([]);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _invoices.Verify(x => x.GetPaymentSummariesAsync(
            It.IsAny<Guid>(),
            It.IsAny<IReadOnlyCollection<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static InvoicePaymentSummary CreatePartialSummary(Invoice invoice)
    {
        var timestamp = new DateTimeOffset(2025, 1, 2, 10, 0, 0, TimeSpan.Zero);
        return InvoicePaymentSummary.Create(
            invoice.TenantId,
            invoice.Id,
            Money.Create(25m, invoice.CurrencyCode),
            Money.Create(invoice.GrossAmount.Amount - 25m, invoice.CurrencyCode),
            paymentBalanceVersion: 1,
            sourceUpdatedAtUtc: timestamp,
            appliedAtUtc: timestamp);
    }
}
