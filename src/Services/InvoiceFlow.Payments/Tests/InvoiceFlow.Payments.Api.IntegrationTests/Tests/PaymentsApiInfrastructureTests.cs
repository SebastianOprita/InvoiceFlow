using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;
using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests;

[Collection(PaymentsApiInfrastructureCollection.Name)]
public sealed class PaymentsApiInfrastructureTests
{
    private readonly TestClient _client;

    public PaymentsApiInfrastructureTests(PaymentsApiFactory factory) => _client = new TestClient(factory);

    [Fact]
    public async Task GetErrors_returns_error_catalogue()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "/api/errors", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var catalogue = await response.Content.ReadFromJsonAsync<ErrorCatalogueResponse>(TestContext.Current.CancellationToken);
        catalogue.Should().NotBeNull();
        catalogue!.DomainErrors.Should().NotBeEmpty();
        catalogue.ApplicationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LiveHealth_returns_healthy()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "/health/live", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("Healthy");
    }

    [Fact]
    public async Task ReadyHealth_returns_healthy()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "/health/ready", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("Healthy");
    }
}
