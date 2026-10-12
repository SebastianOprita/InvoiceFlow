using FluentAssertions;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(AuthApiCollection.Name)]
public sealed class AuthControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl;

    public AuthControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _baseUrl = "/api/tenantId/auth/";
    }

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private string GetUrl(string endpoint)
    {
        return _baseUrl.Replace("tenantId", _factory.Tenant.Id.ToString()) + endpoint;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndTokens()
    {
        // Arrange
        var user = _factory.User;

        // Act
        var request = new LoginRequest(user.Email.Value, TestConstants.Password);
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("login"), null, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await response.Content.ReadFromJsonAsync<LoginUserCommandResponse>(TestContext.Current.CancellationToken);
        loginResult.Should().NotBeNull();
        loginResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = _factory.User;

        // Act
        var request = new LoginRequest(user.Email.Value, "WrongPassword");
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("login"), null, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = _factory.User;
        var loginRequest = new LoginRequest(user.Email.Value, TestConstants.Password);
        var loginResponse = await _client.SendAsync(HttpMethod.Post, GetUrl("login"), null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>(TestContext.Current.CancellationToken);
        var refreshRequest = new RefreshTokenRequest(loginResult!.RefreshToken);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("refresh"), null, refreshRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await response.Content.ReadFromJsonAsync<LoginUserCommandResponse>(TestContext.Current.CancellationToken);
        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        var refreshRequest = new RefreshTokenRequest("invalid-token");
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("refresh"), null, refreshRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidRefreshToken_ShouldReturnNoContent()
    {
        // Arrange
        var user = _factory.User;
        var loginRequest = new LoginRequest(user.Email.Value, TestConstants.Password);

        // Act
        var loginResponse = await _client.SendAsync(HttpMethod.Post, GetUrl("login"), null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>(TestContext.Current.CancellationToken);

        // Assert
        var logoutRequest = new RefreshTokenRequest(loginResult!.RefreshToken);
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("logout"), loginResult!.AccessToken, logoutRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var user = _factory.User;
        var loginRequest = new LoginRequest(user.Email.Value, TestConstants.Password);
        var loginResponse = await _client.SendAsync(HttpMethod.Post, GetUrl("login"), null, loginRequest, TestContext.Current.CancellationToken);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>(TestContext.Current.CancellationToken);
        var changeRequest = new ChangePasswordRequest(TestConstants.Password, "NewPassword123!");

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, GetUrl("change-password"), loginResult!.AccessToken, changeRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Post, "/api/{tenantId}/auth/logout"],
        [HttpMethod.Post, "/api/{tenantId}/auth/change-password"],
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url.Replace("{tenantId}", _factory.Tenant.Id.ToString());

        var response = await _client.SendAsync(method, url, "", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
