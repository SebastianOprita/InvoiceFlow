using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(RolesApiCollection.Name)]
public sealed class RolesApiControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl;

    public RolesApiControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _baseUrl = "/api/tenantId/roles/";
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private string GetAccessToken(SystemPermissions permission = SystemPermissions.ManageRoles)
    {
        return TestJwtTokenFactory.CreateAccessToken(_factory.Tenant.Id, permission, _factory.JwtSettings);
    }

    private string GetUrl(string endpoint)
    {
        return _baseUrl.Replace("tenantId", _factory.Tenant.Id.ToString()) + endpoint;
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok()
    {
        var response = await _client.SendAsync(HttpMethod.Get, GetUrl(""), GetAccessToken(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenRoleExists_Should_ReturnOk()
    {
        // Arrange
        string name = "role";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name);

        // Act
        var response = await _client.SendAsync(HttpMethod.Get, GetUrl($"{createRoleId}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var role = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);

        role.Should().NotBeNull();
        role!.Id.Should().Be(createRoleId);
        role.TenantId.Should().Be(_factory.Tenant.Id);
        role.Name.Should().Be(name);
    }

    [Fact]
    public async Task GetById_WhenRoleDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Get, GetUrl(Guid.CreateVersion7().ToString()), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Should_Return_Created()
    {
        // Arrange
        string name = "role";
        var request = CreateValidRoleRequest(name);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl(""), GetAccessToken(), request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var role = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);
        role.Should().NotBeNull();
        role.TenantId.Should().Be(_factory.Tenant.Id);
        role.Id.Should().NotBeEmpty();
        role.Name.Should().Be(name);
    }

    [Fact]
    public async Task Create_WhenNameAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string name = "role";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name);
        var request = CreateValidRoleRequest(name);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl(""), GetAccessToken(), request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.RoleNameAlreadyExists.Code);
    }

    [Fact]
    public async Task Update_WhenRoleExists_Should_ReturnOk()
    {
        // Arrange
        string name = "role";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name);
        var updateRequest = new UpdateRoleRequest("NewRoleNumber", "Description for RoleNumber3");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, GetUrl(createRoleId.ToString()), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedRole = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);

        updatedRole.Should().NotBeNull();
        updatedRole!.Id.Should().Be(createRoleId);
        updatedRole.TenantId.Should().Be(_factory.Tenant.Id);
        updatedRole.Name.Should().Be("NewRoleNumber");
        updatedRole.Description.Should().Be("Description for RoleNumber3");
    }

    [Fact]
    public async Task Update_WhenRoleDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateRoleRequest("name", "description");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, GetUrl(Guid.CreateVersion7().ToString()), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRoleDoesNotExist_Should_ReturnConflict()
    {
        // Arrange
        string initialName = "role";
        string updateName = "newRole";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, initialName);
        await _factory.InsertRoleAsync(_factory.Tenant.Id, updateName);
        var updateRequest = new UpdateRoleRequest(updateName, "description");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, GetUrl(createRoleId.ToString()), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_WhenRoleExists_Should_ReturnNoContent()
    {
        // Arrange
        string name = "role";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name);

        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl(createRoleId.ToString()), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenRoleDoesNotExists_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl(Guid.CreateVersion7().ToString()), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePermissions_WhenRoleExists_Should_ReturnOk()
    {
        // Arrange
        string name = "role";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name, SystemPermissions.InvoiceRead);
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers | SystemPermissions.ManageRoles);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{createRoleId}/permissions"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRole = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);
        updatedRole.Should().NotBeNull();
        updatedRole!.Id.Should().Be(createRoleId);
        updatedRole.TenantId.Should().Be(_factory.Tenant.Id);
        updatedRole.Permissions.Should().Be((SystemPermissions.ManageUsers | SystemPermissions.ManageRoles).ToPermissions());
    }

    [Fact]
    public async Task UpdatePermissions_WhenRoleDoesNotExists_Should_ReturnNotFound()
    {
        // Assert
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{Guid.CreateVersion7()}/permissions"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GrantPermissions_WhenRoleExists_Should_ReturnOk()
    {
        // Arrange
        string name = "role10";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name, SystemPermissions.InvoiceRead);
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers | SystemPermissions.ManageRoles);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{createRoleId}/permissions/grant"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRole = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);
        updatedRole.Should().NotBeNull();
        updatedRole!.Id.Should().Be(createRoleId);
        updatedRole.TenantId.Should().Be(_factory.Tenant.Id);
        updatedRole.Permissions.Should().Be((SystemPermissions.InvoiceRead | SystemPermissions.ManageUsers | SystemPermissions.ManageRoles).ToPermissions());
    }

    [Fact]
    public async Task GrantPermissions_WhenRoleDoesNotExists_Should_ReturnNotFound()
    {
        // Assert
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{Guid.CreateVersion7()}/permissions/grant"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokePermissions_WhenRoleExists_Should_ReturnOk()
    {
        // Arrange
        string name = "role11";
        var createRoleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, name, SystemPermissions.InvoiceRead | SystemPermissions.ManageUsers | SystemPermissions.ManageRoles);
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers | SystemPermissions.ManageRoles);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{createRoleId}/permissions/revoke"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRole = await response.Content.ReadFromJsonAsync<RoleDto>(TestContext.Current.CancellationToken);
        updatedRole.Should().NotBeNull();
        updatedRole!.Id.Should().Be(createRoleId);
        updatedRole.TenantId.Should().Be(_factory.Tenant.Id);
        updatedRole.Permissions.Should().Be(SystemPermissions.InvoiceRead.ToPermissions());
    }

    [Fact]
    public async Task RevokePermissions_WhenRoleDoesNotExists_Should_ReturnNotFound()
    {
        // Assert
        var updateRequest = new UpdatePermissionsRequest(SystemPermissions.ManageUsers);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{Guid.CreateVersion7()}/permissions/revoke"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Get,    "/api/{tenantId}/roles"],
        [HttpMethod.Get,    "/api/{tenantId}/roles/{roleId}"],
        [HttpMethod.Post,   "/api/{tenantId}/roles"],
        [HttpMethod.Put,    "/api/{tenantId}/roles/{roleId}"],
        [HttpMethod.Delete, "/api/{tenantId}/roles/{roleId}"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions/grant"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions/revoke"]
    ];

    public static IEnumerable<object[]> PermissionProtectedEndpoints =>
    [
        [HttpMethod.Post,   "/api/{tenantId}/roles"],
        [HttpMethod.Put,    "/api/{tenantId}/roles/{roleId}"],
        [HttpMethod.Delete, "/api/{tenantId}/roles/{roleId}"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions/grant"],
        [HttpMethod.Post,   "/api/{tenantId}/roles/{roleId}/permissions/revoke"]
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _factory.Tenant.Id.ToString())
            .Replace("{roleId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, null, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(PermissionProtectedEndpoints))]
    public async Task Endpoints_Should_Return_Forbidden_When_Missing_ManageRoles_Permission(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _factory.Tenant.Id.ToString())
            .Replace("{roleId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(
            method,
            url,
            GetAccessToken(SystemPermissions.None),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static CreateRoleRequest CreateValidRoleRequest(string name)
    {
        return new CreateRoleRequest(name, "Description for " + name, SystemPermissions.ManageRoles);
    }
}
