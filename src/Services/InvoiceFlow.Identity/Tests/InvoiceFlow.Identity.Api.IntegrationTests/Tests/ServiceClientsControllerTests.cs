using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using InvoiceFlow.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(ServiceClientsApiCollection.Name)]
public sealed class ServiceClientsControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl = "/api/platform/service-clients/";
    private readonly string _accessToken;

    public ServiceClientsControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _accessToken = TestJwtTokenFactory.CreatePlatformUserAccessToken(factory.PlatformUser.Id, factory.JwtSettings);
    }

    public async ValueTask InitializeAsync() => await _factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetSecrets_ReturnsMetadataAndNotFoundForMissingClient()
    {
        var (client, _, secret) = await CreateServiceClientAsync(includeSecret: true);

        var response = await _client.SendAsync(HttpMethod.Get, $"{_baseUrl}{client.Id}/secrets", _accessToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var secrets = await response.Content.ReadFromJsonAsync<ServiceClientSecretDto[]>(TestContext.Current.CancellationToken);
        secrets.Should().ContainSingle();
        secrets![0].Id.Should().Be(secret!.Id);
        secrets[0].Name.Should().Be("existing");
        secrets[0].Description.Should().Be("description");
        typeof(ServiceClientSecretDto).GetProperties().Select(x => x.Name).Should().NotContain(nameof(ServiceClientSecret.SecretHash));

        var missingResponse = await _client.SendAsync(HttpMethod.Get, $"{_baseUrl}{Guid.CreateVersion7()}/secrets", _accessToken, TestContext.Current.CancellationToken);
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndRevokeSecret_ReturnExpectedResponses()
    {
        var (client, _, _) = await CreateServiceClientAsync();
        var request = new CreateServiceClientSecretRequest("integration-key", "test secret", GetNow().AddDays(1));

        var createResponse = await _client.SendAsync(HttpMethod.Post, $"{_baseUrl}{client.Id}/secrets", _accessToken, request, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateServiceClientSecretCommandResponse>(TestContext.Current.CancellationToken);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Secret.Should().NotBeNullOrWhiteSpace();
        created.Name.Should().Be("integration-key");

        var getResponse = await _client.SendAsync(HttpMethod.Get, $"{_baseUrl}{client.Id}/secrets", _accessToken, TestContext.Current.CancellationToken);
        var savedSecrets = await getResponse.Content.ReadFromJsonAsync<ServiceClientSecretDto[]>(TestContext.Current.CancellationToken);
        savedSecrets.Should().ContainSingle();
        savedSecrets![0].Id.Should().Be(created.Id);

        var revokeResponse = await _client.SendAsync(HttpMethod.Delete, $"{_baseUrl}{client.Id}/secrets/{savedSecrets[0].Id}", _accessToken, TestContext.Current.CancellationToken);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GrantAndRevokePermissions_UpdateServiceClient()
    {
        var (client, _, _) = await CreateServiceClientAsync();
        const long permission = 1L << 3;

        var grantResponse = await _client.SendAsync(HttpMethod.Post, $"{_baseUrl}{client.Id}/permissions", _accessToken, new GrantServiceClientPermissionsRequest(permission), TestContext.Current.CancellationToken);
        grantResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ReloadClientAsync(client.Id)).HasPermission(permission).Should().BeTrue();

        var revokeResponse = await _client.SendAsync(HttpMethod.Delete, $"{_baseUrl}{client.Id}/permissions/{permission}", _accessToken, TestContext.Current.CancellationToken);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ReloadClientAsync(client.Id)).HasPermission(permission).Should().BeFalse();
    }

    [Fact]
    public async Task GrantAndRevokeTenant_UpdateServiceClient()
    {
        var (client, _, _) = await CreateServiceClientAsync();
        var grantResponse = await _client.SendAsync(HttpMethod.Post, $"{_baseUrl}{client.Id}/tenants", _accessToken, new GrantServiceClientTenantRequest(_factory.Tenant.Id), TestContext.Current.CancellationToken);
        grantResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ReloadClientAsync(client.Id)).CanAccessTenant(_factory.Tenant.Id).Should().BeTrue();

        var revokeResponse = await _client.SendAsync(HttpMethod.Delete, $"{_baseUrl}{client.Id}/tenants/{_factory.Tenant.Id}", _accessToken, TestContext.Current.CancellationToken);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ReloadClientAsync(client.Id)).CanAccessTenant(_factory.Tenant.Id).Should().BeFalse();
    }

    [Fact]
    public async Task ServiceClientEndpoints_RequirePlatformAuthentication()
    {
        var response = await _client.SendAsync(HttpMethod.Get, $"{_baseUrl}{Guid.CreateVersion7()}/secrets", null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(ServiceClient Client, Organization Organization, ServiceClientSecret? Secret)> CreateServiceClientAsync(bool includeSecret = false)
    {
        var now = GetNow();
        var organization = Organization.Create(Guid.CreateVersion7(), OrganizationName.Create("Integration Org"), OrganizationTypes.Partner, now);
        var uniqueClientId = $"integration-{Guid.CreateVersion7():N}";
        var serviceClient = ServiceClient.Create(Guid.CreateVersion7(), organization, ServiceClientIdentifier.Create(uniqueClientId), ServiceClientName.Create("Integration Client"), now);
        var secret = includeSecret
            ? serviceClient.AddSecret(Guid.CreateVersion7(), ServiceClientSecretHash.Create(new string('A', 64)), ServiceClientSecretName.Create("existing"), ServiceClientSecretDescription.Create("description"), now.AddDays(1), now)
            : null;
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Organizations.Add(organization);
        db.ServiceClients.Add(serviceClient);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (serviceClient, organization, secret);
    }

    private async Task<ServiceClient> ReloadClientAsync(Guid clientId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        return (await db.ServiceClients.Include(x => x.Tenants).SingleAsync(x => x.Id == clientId, TestContext.Current.CancellationToken));
    }

    private DateTimeOffset GetNow()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>().GetUtcNow();
    }
}
