using FluentAssertions;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(TenantsApiCollection.Name)]
public sealed class TenantsControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private string BaseUrl =>
        $"/api/platform/tenants/";
    public readonly string AccessToken;

    public TenantsControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        AccessToken = TestJwtTokenFactory.CreatePlatformUserAccessToken(_factory.PlatformUser.Id, _factory.JwtSettings);
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
    public async Task GetAll_Should_Return_Ok()
    {
        var response = await _client.SendAsync(HttpMethod.Get, BaseUrl, AccessToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenUserExists_Should_ReturnOk()
    {
        // Act
        var tenantId = _factory.Tenant.Id;
        var response = await _client.SendAsync(HttpMethod.Get, BaseUrl + $"{tenantId}", AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user!.Id.Should().Be(tenantId);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Get, BaseUrl + $"{Guid.CreateVersion7()}", AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Should_Return_Created()
    {
        // Arrange
        string name = "Test Tenant2";
        string slug = "test-tenant2";

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, BaseUrl, AccessToken, CreateValidTenantRequest(name, slug));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);
        tenant.Should().NotBeNull();
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be(name);
        tenant.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task Create_WhenTenantSlugAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string name = "Test3 Tenant3";
        string slug = "test3-tenant3";
        var createTenantId = _factory.InsertTenant(name, slug);

        var request = CreateValidTenantRequest(name, slug);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, BaseUrl, AccessToken, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.TenantSlugAlreadyExists.Code);
    }

    [Fact]
    public async Task Update_WhenTenantExists_Should_ReturnOk()
    {
        // Arrange
        string name = "Test4 Tenant4";
        string slug = "test4-tenant4";
        var createTenantId = _factory.InsertTenant(name, slug);

        var updateRequest = new UpdateTenantRequest("Test Tenant4 Updated");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, BaseUrl + $"{createTenantId}", AccessToken, updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTenant = await response.Content
            .ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        updatedTenant.Should().NotBeNull();
        updatedTenant!.Id.Should().Be(createTenantId);
        updatedTenant.Name.Should().Be("Test Tenant4 Updated");
    }

    [Fact]
    public async Task Update_WhenTenantDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateTenantRequest("Test Tenant Updated");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, BaseUrl + $"{Guid.CreateVersion7()}", AccessToken, updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenTenantIsInactive_Should_ReturnOk()
    {
        // Arrange
        string name = "Test Tenant5";
        string slug = "test-tenant5";
        var createTenantId = _factory.InsertTenant(name, slug);

        // deactivate first
        var deactivateResponse = await _client.SendAsync(HttpMethod.Delete, BaseUrl + $"deactivate/{createTenantId}", AccessToken);

        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var activateResponse = await _client.SendAsync(HttpMethod.Post, BaseUrl + $"activate/{createTenantId}", AccessToken);

        // Assert
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await _client.SendAsync(HttpMethod.Get, BaseUrl + $"{createTenantId}", AccessToken);

        var updatedTenant = await updatedResponse.Content
            .ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        updatedTenant.Should().NotBeNull();
        updatedTenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenTenantDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, BaseUrl + $"activate/{Guid.CreateVersion7()}", AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenTenantIsInactive_Should_ReturnOk()
    {
        // Arrange
        string name = "Test Tenant6";
        string slug = "test-tenant6";
        var createTenantId = _factory.InsertTenant(name, slug);

        // Act
        var deactivateResponse = await _client.SendAsync(HttpMethod.Delete, BaseUrl + $"deactivate/{createTenantId}", AccessToken);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert
        var updatedResponse = await _client.SendAsync(HttpMethod.Get, BaseUrl + $"{createTenantId}", AccessToken);

        var updatedTenant = await updatedResponse.Content
            .ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        updatedTenant.Should().NotBeNull();
        updatedTenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenTenantDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, BaseUrl + $"deactivate/{Guid.CreateVersion7()}", AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Get,    "/api/platform/tenants"],
        [HttpMethod.Get,    "/api/platform/tenants/{tenantId}"],
        [HttpMethod.Post,   "/api/platform/tenants"],
        [HttpMethod.Put,    "/api/platform/tenants/{tenantId}"],
        [HttpMethod.Post,   "/api/platform/tenants/activate/{tenantId}"],
        [HttpMethod.Delete, "/api/platform/tenants/deactivate/{tenantId}"],
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url.Replace("{tenantId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, "");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static CreateTenantRequest CreateValidTenantRequest(string name, string slug)
    {
        return new CreateTenantRequest
        (
            name,
            slug
        );
    }
}
