using System.Net;
using FluentAssertions;
using InvoiceFlow.Products.Api.IntegrationTests.BaseTests;
using Xunit;

namespace InvoiceFlow.Products.Api.IntegrationTests;

[Collection(ProductsApiInfrastructureCollection.Name)]
public sealed class ProductsApiInfrastructureTests
{
    private readonly TestClient _client;

    public ProductsApiInfrastructureTests(ProductsApiFactory factory)
    {
        _client = new TestClient(factory);
    }

    [Fact]
    public async Task GetErrors_Should_ReturnOk()
    {
        var response = await _client.SendAsync(
            method: HttpMethod.Get,
            url: "/api/errors",
            token: null,
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LiveHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            method: HttpMethod.Get,
            url: "/health/live",
            token: null,
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task ReadyHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            method: HttpMethod.Get,
            url: "/health/ready",
            token: null,
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        content.Should().Be("Healthy");
    }
}
