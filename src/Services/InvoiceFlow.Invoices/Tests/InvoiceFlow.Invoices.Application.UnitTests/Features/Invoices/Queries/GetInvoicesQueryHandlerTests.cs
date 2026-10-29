using FluentAssertions;
using Moq;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoicesQueryHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoices = new();
    private readonly GetInvoicesQueryHandler _sut;

    public GetInvoicesQueryHandlerTests()
    {
        _sut = new GetInvoicesQueryHandler(_invoices.Object);
    }

    [Fact]
    public async Task Handle_ReturnsInvoiceDtos_ForTenant()
    {
        var first = InvoiceTestData.DraftInvoice();
        var second = InvoiceTestData.DraftInvoice();
        var query = new GetInvoicesQuery(InvoiceTestData.TenantId);
        _invoices.Setup(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None))
            .ReturnsAsync([first, second]);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Id).Should().Contain([first.Id, second.Id]);
        result.Value.Should().OnlyContain(x => x.Lines.Count == 1);
        result.Value.Single(x => x.Id == first.Id).Lines.Single().Id.Should().Be(first.Lines.Single().Id);
        result.Value.Single(x => x.Id == second.Id).Lines.Single().Id.Should().Be(second.Lines.Single().Id);
        _invoices.Verify(x => x.GetAllInvoicesWithLinesAsync(query.TenantId, CancellationToken.None), Times.Once);
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
    }
}
