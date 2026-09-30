using FluentAssertions;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using System.Net;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(IdentityApiInfrastructureCollection.Name)]
public sealed class IdentityApiInfrastructureTests
{
    private readonly TestClient _client;

    public IdentityApiInfrastructureTests(IdentityApiFactory factory)
    {
        _client = new TestClient(factory);
    }

    [Fact]
    public async Task GetErrors_Should_ReturnOk()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/api/errors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LiveHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task ReadyHealth_Should_ReturnHealthy()
    {
        var response = await _client.SendAsync(
            HttpMethod.Get,
            "/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        content.Should().Be("Healthy");
    }
}
