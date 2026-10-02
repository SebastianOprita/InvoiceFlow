using FluentAssertions;
using InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
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

    [Fact]
    public async Task Application_Should_FailToStart_WhenJwtSecretIsMissing()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["JwtSettings:Issuer"] = "InvoiceFlow",
                            ["JwtSettings:Audience"] = "InvoiceFlow",
                            ["JwtSettings:Secret"] = null
                        });

                });
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                using var client = factory.CreateClient();

                await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
            });

        exception.Message.Should()
            .Be("JwtSettings:Secret is not configured.");
    }
}