using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(UsersApiCollection.Name)]
public sealed class UsersControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl;

    public UsersControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _baseUrl = "/api/tenantId/users/";
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private string GetAccessToken(SystemPermissions permission = SystemPermissions.RoleRead)
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
    public async Task GetById_Should_ReturnOk()
    {
        // Arrange && Act
        var response = await _client.SendAsync(HttpMethod.Get, GetUrl($"{_factory.User.Id}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>(TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user!.Id.Should().Be(_factory.User.Id);
        user.TenantId.Should().Be(_factory.Tenant.Id);
        user.Email.Should().Be(_factory.User.Email.Value);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_Should_ReturnNotFound()
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
        string email = "test@example.com";

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl(""), GetAccessToken(), CreateValidUserRequest(email), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.TenantId.Should().Be(_factory.Tenant.Id);
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task Create_WhenUserEmailAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        string email = _factory.User.Email.Value;

        var request = CreateValidUserRequest(email);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl(""), GetAccessToken(), CreateValidUserRequest(email), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.UserEmailAlreadyExists.Code);
    }

    [Fact]
    public async Task Update_Should_ReturnOk()
    {
        // Arrange
        var userId = _factory.User.Id;
        var updateRequest = new UpdateUserRequest
            ("FirstName", "LastName");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, GetUrl($"{userId}"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser.Id.Should().Be(userId);
        updatedUser.Email.Should().Be(_factory.User.Email.Value);
        updatedUser.TenantId.Should().Be(_factory.Tenant.Id);
        updatedUser.FirstName.Should().Be("FirstName");
        updatedUser.LastName.Should().Be("LastName");
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
            ("FirstName", "LastName");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, GetUrl($"{Guid.CreateVersion7()}"), GetAccessToken(), updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_Should_ReturnOk()
    {
        // Arrange
        string email = "testUser@example.com";
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, email, isActive: false);

        // Act
        var activateResponse = await _client.SendAsync(HttpMethod.Post, GetUrl($"activate/{createUserId}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await _client.SendAsync(HttpMethod.Get, GetUrl($"{createUserId}"), GetAccessToken(), TestContext.Current.CancellationToken);

        var updatedUser = await updatedResponse.Content.ReadFromJsonAsync<UserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"activate/{Guid.CreateVersion7()}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_Should_ReturnOk()
    {
        // Arrange
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, "testUser@example.com");

        // Act
        var deactivateResponse = await _client.SendAsync(HttpMethod.Delete, GetUrl($"deactivate/{createUserId}"), GetAccessToken(), TestContext.Current.CancellationToken);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert
        var updatedResponse = await _client.SendAsync(HttpMethod.Get, GetUrl($"{createUserId}"), GetAccessToken(), TestContext.Current.CancellationToken);

        var updatedUser = await updatedResponse.Content.ReadFromJsonAsync<UserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl($"deactivate/{Guid.CreateVersion7()}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRoles_Should_ReturnOk()
    {
        // Assert
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, "testUser@example.com");
        var roleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, "TestRole1");

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{createUserId}/roles"), GetAccessToken(), new AssignRoleRequest(roleId), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignRoles_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{Guid.CreateVersion7()}/roles"), GetAccessToken(), new AssignRoleRequest(Guid.CreateVersion7()), TestContext.Current.CancellationToken);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        body.Should().NotBeNull();
        body.Title.Should().Be(ApplicationErrors.UserNotFound.Code);
    }

    [Fact]
    public async Task AssignRoles_WhenRoleDoesNotExist_Should_ReturnNotFound()
    {
        // Assert
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, "testUser@example.com");
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl($"{createUserId}/roles"), GetAccessToken(), new AssignRoleRequest(Guid.CreateVersion7()), TestContext.Current.CancellationToken);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.RoleNotFound.Code);
    }

    [Fact]
    public async Task RevokeRole_Should_ReturnOk()
    {
        // Assert
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, "testUser@example.com");
        var roleId = await _factory.InsertRoleAsync(_factory.Tenant.Id, "TestRole");

        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl($"{createUserId}/roles/{roleId}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RevokeRole_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl($"{Guid.CreateVersion7()}/roles/{Guid.CreateVersion7()}"), GetAccessToken(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.UserNotFound.Code);
    }

    [Fact]
    public async Task RevokeRole_WhenRoleDoesNotExist_Should_ReturnNotFound()
    {
        // Assert
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, "testUser@example.com");
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, GetUrl($"{createUserId}/roles/{Guid.CreateVersion7()}"), GetAccessToken(), TestContext.Current.CancellationToken);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.RoleNotFound.Code);
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Get,    "/api/{tenantId}/users"],
        [HttpMethod.Get,    "/api/{tenantId}/users/{userId}"],
        [HttpMethod.Post,   "/api/{tenantId}/users"],
        [HttpMethod.Put,    "/api/{tenantId}/users/{userId}"],
        [HttpMethod.Post,   "/api/{tenantId}/users/activate/{userId}"],
        [HttpMethod.Delete, "/api/{tenantId}/users/deactivate/{userId}"],
        [HttpMethod.Post,   "/api/{tenantId}/users/{userId}/roles"],
        [HttpMethod.Delete, "/api/{tenantId}/users/{userId}/roles/{roleId}"]
    ];

    public static IEnumerable<object[]> PermissionProtectedEndpoints =>
    [
        [HttpMethod.Post,   "/api/{tenantId}/users"],
        [HttpMethod.Put,    "/api/{tenantId}/users/{userId}"],
        [HttpMethod.Post,   "/api/{tenantId}/users/activate/{userId}"],
        [HttpMethod.Delete, "/api/{tenantId}/users/deactivate/{userId}"],
        [HttpMethod.Post,   "/api/{tenantId}/users/{userId}/roles"],
        [HttpMethod.Delete, "/api/{tenantId}/users/{userId}/roles/{roleId}"]
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _factory.Tenant.Id.ToString())
            .Replace("{userId}", Guid.CreateVersion7().ToString())
            .Replace("{roleId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, "", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(PermissionProtectedEndpoints))]
    public async Task Endpoints_Should_Return_Forbidden_When_Missing_ManageUsers_Permission(
        HttpMethod method,
        string url)
    {
        url = url
            .Replace("{tenantId}", _factory.Tenant.Id.ToString())
            .Replace("{userId}", Guid.CreateVersion7().ToString())
            .Replace("{roleId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(
            method,
            url,
            GetAccessToken(SystemPermissions.None),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static CreateUserRequest CreateValidUserRequest(string email)
    {
        return new CreateUserRequest
        (
            email,
            "Password123!",
            "John",
            "Doe"
        );
    }
}
