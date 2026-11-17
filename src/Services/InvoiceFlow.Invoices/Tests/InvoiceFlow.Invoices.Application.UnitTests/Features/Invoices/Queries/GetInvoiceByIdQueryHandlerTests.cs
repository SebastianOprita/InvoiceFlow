using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoiceByIdQueryHandlerTests
{
    private readonly Mock<IInvoicesRepository> _invoices = new();
    private readonly GetInvoiceByIdQueryHandler _sut;

    public GetInvoiceByIdQueryHandlerTests()
    {
        _sut = new GetInvoiceByIdQueryHandler(_invoices.Object);
    }

    [Fact]
    public async Task Handle_ReturnsInvoiceDto_WhenInvoiceExists()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        var query = new GetInvoiceByIdQuery(invoice.TenantId, invoice.Id);
        _invoices.Setup(x => x.GetInvoiceByIdWithLinesAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync(invoice);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(invoice.Id);
        result.Value.CustomerCode.Should().Be(invoice.CustomerCode.Value);
        result.Value.Lines.Should().HaveCount(1);
        result.Value.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        result.Value.PaidAmount.Should().Be(0m);
        result.Value.RemainingAmount.Should().Be(invoice.GrossAmount.Amount);
        _invoices.Verify(x => x.GetInvoiceByIdWithLinesAsync(query.TenantId, query.InvoiceId, CancellationToken.None), Times.Once);
        _invoices.Verify(x => x.GetPaymentSummaryAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComposesPaymentFieldsFromSummary_WhenSummaryExists()
    {
        var invoice = InvoiceTestData.DraftInvoice();
        invoice.Issue(InvoiceNumber.Create("INV-001"), new DateOnly(2025, 1, 1), new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var query = new GetInvoiceByIdQuery(invoice.TenantId, invoice.Id);
        var timestamp = new DateTimeOffset(2025, 1, 2, 10, 0, 0, TimeSpan.Zero);
        var summary = InvoicePaymentSummary.Create(
            invoice.TenantId,
            invoice.Id,
            Money.Create(25m, invoice.CurrencyCode),
            Money.Create(invoice.GrossAmount.Amount - 25m, invoice.CurrencyCode),
            paymentBalanceVersion: 1,
            sourceUpdatedAtUtc: timestamp,
            appliedAtUtc: timestamp);

        _invoices.Setup(x => x.GetInvoiceByIdWithLinesAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync(invoice);
        _invoices.Setup(x => x.GetPaymentSummaryAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync(summary);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PaymentStatus.Should().Be(InvoicePaymentStatus.PartiallyPaid);
        result.Value.PaidAmount.Should().Be(25m);
        result.Value.RemainingAmount.Should().Be(invoice.GrossAmount.Amount - 25m);
        _invoices.Verify(x => x.GetPaymentSummaryAsync(query.TenantId, query.InvoiceId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        var query = new GetInvoiceByIdQuery(InvoiceTestData.TenantId, Guid.CreateVersion7());
        _invoices.Setup(x => x.GetInvoiceByIdWithLinesAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync((InvoiceFlow.Invoices.Domain.Invoice?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoiceNotFound);
    }
}
