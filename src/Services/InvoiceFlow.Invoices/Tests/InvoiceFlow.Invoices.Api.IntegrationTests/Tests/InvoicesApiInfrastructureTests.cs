using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;
using Xunit;

namespace InvoiceFlow.Invoices.Api.IntegrationTests;

[Collection(InvoicesApiInfrastructureCollection.Name)]
public sealed class InvoicesApiInfrastructureTests
{
    private readonly TestClient _client;

    public InvoicesApiInfrastructureTests(InvoicesApiFactory factory)
    {
        _client = new TestClient(factory);
    }

    [Fact]
    public async Task GetErrors_Should_ReturnOk()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/api/errors",
            token: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var catalogue = await response.Content.ReadFromJsonAsync<ErrorCatalogueResponse>(
            TestContext.Current.CancellationToken);
        catalogue.Should().NotBeNull();
        catalogue!.DomainErrors.Should().NotBeEmpty();
        catalogue.ApplicationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LiveHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/health/live",
            token: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().Be("Healthy");
    }

    [Fact]
    public async Task ReadyHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/health/ready",
            token: null,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().Be("Healthy");
    }
}
