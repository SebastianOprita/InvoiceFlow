using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoiceByIdQueryHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
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
        _invoices.Setup(x => x.GetInvoiceByIdAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync(invoice);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(invoice.Id);
        result.Value.CustomerCode.Should().Be(invoice.CustomerCode.Value);
        result.Value.Lines.Should().HaveCount(1);
        _invoices.Verify(x => x.GetInvoiceByIdAsync(query.TenantId, query.InvoiceId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        var query = new GetInvoiceByIdQuery(InvoiceTestData.TenantId, Guid.CreateVersion7());
        _invoices.Setup(x => x.GetInvoiceByIdAsync(query.TenantId, query.InvoiceId, CancellationToken.None))
            .ReturnsAsync((InvoiceFlow.Invoices.Domain.Invoice?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Error.Should().Be(ApplicationErrors.InvoiceNotFound);
    }
}
