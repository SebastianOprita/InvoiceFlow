using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests;

[Collection(CustomersApiInfrastructureCollection.Name)]
public sealed class CustomersApiInfrastructureTests
{
    private readonly TestClient _client;

    public CustomersApiInfrastructureTests(CustomersApiFactory factory)
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

        var catalogue = await response.Content.ReadFromJsonAsync<ErrorCatalogueResponse>(TestContext.Current.CancellationToken);
        catalogue.Should().NotBeNull();
        catalogue!.DomainErrors.Should().NotBeEmpty();
        catalogue.ApplicationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LiveHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            method: HttpMethod.Get,
            url: "/health/live",
            token:null,
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
