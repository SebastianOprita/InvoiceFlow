using FluentAssertions;
using InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

    [Fact]
    public void Application_Should_FailToStart_WhenJwtSecretIsMissing()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
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

        // Act
        var exception = Assert.Throws<OptionsValidationException>(
            () => factory.CreateClient());

        // Assert
        exception.Message.Should()
            .Contain("JwtSettings:Secret is not configured.");
    }
}