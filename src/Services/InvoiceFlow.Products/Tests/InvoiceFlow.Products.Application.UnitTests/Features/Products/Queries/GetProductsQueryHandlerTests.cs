using FluentAssertions;
using InvoiceFlow.Products.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Products.Application.UnitTests.Features.Queries;

public sealed class GetProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductDtos()
    {
        var tenantId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var products = new List<Product>
        {
            CreateProduct(tenantId, Guid.NewGuid(), "PROD-001", "First product"),
            CreateProduct(tenantId, Guid.NewGuid(), "PROD-002", "Second product")
        };

        var repository = new Mock<IProductsRepository>();

        repository
            .Setup(x => x.GetAllProductsAsync(tenantId, cancellationToken))
            .ReturnsAsync(products);

        var handler = new GetProductsQueryHandler(repository.Object);
        var query = new GetProductsQuery(tenantId);

        var result = await handler.Handle(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].TenantId.Should().Be(tenantId);
        result.Value[0].ProductCode.Should().Be("PROD-001");
        result.Value[0].ProductName.Should().Be("First product");
        result.Value[1].ProductCode.Should().Be("PROD-002");
        result.Value[1].ProductName.Should().Be("Second product");

        repository.Verify(
            x => x.GetAllProductsAsync(tenantId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsNoProducts()
    {
        var tenantId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var repository = new Mock<IProductsRepository>();

        repository
            .Setup(x => x.GetAllProductsAsync(tenantId, cancellationToken))
            .ReturnsAsync([]);

        var handler = new GetProductsQueryHandler(repository.Object);
        var query = new GetProductsQuery(tenantId);

        var result = await handler.Handle(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    private static Product CreateProduct(
        Guid tenantId,
        Guid productId,
        string code,
        string name)
    {
        return Product.Create(
            tenantId,
            productId,
            ProductCode.Create(code),
            ProductName.Create(name),
            null,
            Enum.GetValues<UnitOfMeasure>()[0],
            Money.Create(12.34m, CurrencyCode.Create("USD")),
            Percentage.Create(20m),
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    }
}
