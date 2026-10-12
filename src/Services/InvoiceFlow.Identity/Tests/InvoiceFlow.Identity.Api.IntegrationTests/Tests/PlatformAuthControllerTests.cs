using FluentAssertions;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;


[Collection(PlatformAuthApiCollection.Name)]
public sealed class PlatformAuthControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl;

    public PlatformAuthControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _baseUrl = $"/api/platform/auth/";
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
    public async Task Login_WithValidCredentials_ShouldReturnOkAndTokens()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;

        // Act
        var request = new LoginRequest(platformUser.Email.Value, TestConstants.Password);
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await response.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);
        loginResult.Should().NotBeNull();
        loginResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;

        // Act
        var request = new LoginRequest(platformUser.Email.Value, "WrongPassword");
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;
        var loginRequest = new LoginRequest(platformUser.Email.Value, TestConstants.Password);
        var loginResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);
        var refreshRequest = new RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "refresh", null, refreshRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await response.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);
        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        var refreshRequest = new RefreshTokenRequest("invalid-token");
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "refresh", null, refreshRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ShouldReturnNoContent()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;
        var loginRequest = new LoginRequest(platformUser.Email.Value, TestConstants.Password);

        // Act
        var loginResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);

        // Assert
        var logoutRequest = new RefreshTokenRequest(loginResult!.RefreshToken);
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "logout", loginResult!.AccessToken, logoutRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;
        var loginRequest = new LoginRequest(platformUser.Email.Value, TestConstants.Password);
        var loginResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);
        var changeRequest = new ChangePasswordRequest(TestConstants.Password, "NewPassword123!");

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + "change-password", loginResult!.AccessToken, changeRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Impersonate_ShouldReturnOkAndTokens()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;
        var user = _factory.User;

        // Act
        var loginRequest = new LoginRequest(platformUser.Email.Value, TestConstants.Password);
        var loginResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + "login", null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginPlatformUserCommandResponse>(TestContext.Current.CancellationToken);

        var impersonationRequest = new ImpersonateUserRequest(_factory.Tenant.Id, user.Email.Value, "reason");
        var impersonationResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + "impersonate-user", loginResult!.AccessToken, impersonationRequest, TestContext.Current.CancellationToken);

        // Assert
        impersonationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var impersonationResult = await impersonationResponse.Content.ReadFromJsonAsync<ImpersonateUserCommandResponse>(TestContext.Current.CancellationToken);
        impersonationResult.Should().NotBeNull();
        impersonationResult!.ImpersonationToken.Should().NotBeNullOrWhiteSpace();
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Post, "/api/platform/auth/logout"],
        [HttpMethod.Post, "/api/platform/auth/change-password"],
        [HttpMethod.Post, "/api/platform/auth/impersonate-user"]
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        var response = await _client.SendAsync(method, url, "", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
