using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Domain;
using InvoiceFlow.Identity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(TokenApiCollection.Name)]
public sealed class TokenControllerTests : IAsyncLifetime
{
    private const string ClientId = "integration-token-client";
    private const string ClientSecret = "integration-client-secret";
    private readonly IdentityApiFactory _factory;
    private readonly HttpClient _client;
    private Guid _tenantId;

    public TokenControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _tenantId = _factory.Tenant.Id;
        await SeedServiceClientAsync();
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Token_WithValidClientCredentials_ReturnsBearerToken()
    {
        using var response = await RequestTokenAsync("client_credentials", ClientId, ClientSecret, _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        body.RootElement.GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("token_type").GetString().Should().Be("Bearer");
        body.RootElement.GetProperty("expires_in").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Token_WithUnsupportedGrantType_ReturnsOAuthError()
    {
        using var response = await RequestTokenAsync("password", ClientId, ClientSecret, _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertOAuthErrorAsync(response, "unsupported_grant_type");
    }

    [Theory]
    [InlineData("", ClientSecret)]
    [InlineData(ClientId, "")]
    [InlineData("unknown-client", ClientSecret)]
    [InlineData(ClientId, "wrong-secret")]
    public async Task Token_WithInvalidCredentials_ReturnsInvalidClient(string clientId, string clientSecret)
    {
        using var response = await RequestTokenAsync("client_credentials", clientId, clientSecret, _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await AssertOAuthErrorAsync(response, "invalid_client");
    }

    [Fact]
    public async Task Token_WithoutTenantAuthorization_ReturnsUnauthorizedClient()
    {
        var unauthorizedTenantId = Guid.CreateVersion7();
        using var response = await RequestTokenAsync("client_credentials", ClientId, ClientSecret, unauthorizedTenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertOAuthErrorAsync(response, "unauthorized_client");
    }

    private async Task<HttpResponseMessage> RequestTokenAsync(string grantType, string clientId, string clientSecret, Guid tenantId)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = grantType,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["tenant_id"] = tenantId.ToString()
        });

        return await _client.PostAsync("/connect/token", content, TestContext.Current.CancellationToken);
    }

    private static async Task AssertOAuthErrorAsync(HttpResponseMessage response, string expectedError)
    {
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        body.RootElement.GetProperty("error").GetString().Should().Be(expectedError);
    }

    private async Task SeedServiceClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<IdentityDbContext>();
        var now = services.GetRequiredService<ISystemDateTimeProvider>().GetUtcNow();
        var organization = Organization.Create(Guid.CreateVersion7(), OrganizationName.Create("Token Integration Org"), OrganizationTypes.Partner, now);
        var client = ServiceClient.Create(Guid.CreateVersion7(), organization, ServiceClientIdentifier.Create(ClientId), ServiceClientName.Create("Token Integration Client"), now);
        client.GrantTenant(_tenantId, now);

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ClientSecret)));
        client.AddSecret(Guid.CreateVersion7(), ServiceClientSecretHash.Create(hash), ServiceClientSecretName.Create("integration"), null, now.AddDays(1), now);

        db.Organizations.Add(organization);
        db.ServiceClients.Add(client);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
