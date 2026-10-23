using FluentAssertions;
using InvoiceFlow.Products.Application.Features;
using InvoiceFlow.Products.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Products.Application.UnitTests.Features.Queries;

public sealed class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductDto_WhenProductExists()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var product = CreateProduct(tenantId, productId);
        var repository = new Mock<IProductsRepository>();

        repository
            .Setup(x => x.GetProductByIdAsync(
                tenantId,
                productId,
                cancellationToken))
            .ReturnsAsync(product);

        var handler = new GetProductByIdQueryHandler(repository.Object);
        var query = new GetProductByIdQuery(tenantId, productId);

        var result = await handler.Handle(query, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Id.Should().Be(productId);
        result.Value.ProductCode.Should().Be("PROD-001");
        result.Value.ProductName.Should().Be("Product name");

        repository.Verify(x => x.GetProductByIdAsync(
            tenantId,
            productId,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var repository = new Mock<IProductsRepository>();

        repository
            .Setup(x => x.GetProductByIdAsync(
                tenantId,
                productId,
                cancellationToken))
            .ReturnsAsync((Product?)null);

        var handler = new GetProductByIdQueryHandler(repository.Object);
        var query = new GetProductByIdQuery(tenantId, productId);

        var result = await handler.Handle(query, cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ApplicationErrors.ProductNotFound);

        repository.Verify(x => x.GetProductByIdAsync(
            tenantId,
            productId,
            cancellationToken), Times.Once);
    }

    private static Product CreateProduct(Guid tenantId, Guid productId)
    {
        return Product.Create(
            tenantId,
            productId,
            ProductCode.Create("PROD-001"),
            ProductName.Create("Product name"),
            ProductDescription.Create("Product description"),
            Enum.GetValues<UnitOfMeasure>()[0],
            Money.Create(12.34m, CurrencyCode.Create("USD")),
            Percentage.Create(20m),
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    }
}
