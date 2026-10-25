using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Products.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Products.Application;
using Xunit;

namespace InvoiceFlow.Products.Api.IntegrationTests;

[Collection(ProductsApiCollection.Name)]
public sealed class ProductsApiTests : IAsyncLifetime
{
    private readonly ProductsApiFactory _factory;
    private readonly TestClient _client;
    private readonly Guid _tenantId;
    private readonly string _baseUrl;
    private readonly string _productAccessToken;

    public ProductsApiTests(ProductsApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _tenantId = Guid.CreateVersion7();
        _baseUrl = $"/api/{_tenantId}/products/";
        _productAccessToken = TestJwtTokenFactory.CreateAccessToken(
            _tenantId,
            SystemPermissions.ProductRead,
            _factory.JwtSettings);
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetAll_Should_ReturnOk()
    {
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl, _productAccessToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenProductExists_Should_ReturnOk()
    {
        // Arrange
        string productCode = "TestProduct";
        var productId = await _factory.InsertProductAsync(_tenantId, productCode);

        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + productId, _productAccessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(TestContext.Current.CancellationToken);

        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
        product.TenantId.Should().Be(_tenantId);
        product.ProductCode.Should().Be(productCode);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + Guid.CreateVersion7(), _productAccessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
