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
    private string BaseUrl =>
        $"/api/{_factory.Tenant.Id}/auth/";

    public AuthControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
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
        string email = "test@example.com";
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, email, TestConstants.PasswordHash);

        // Act
        var request = new LoginRequest(email, TestConstants.Password);
        var response = await _client.SendAsync(HttpMethod.Post, BaseUrl + "login", request);

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
        string email = "test@example.com";
        var createUserId = await _factory.InsertUserAsync(_factory.Tenant.Id, email, TestConstants.PasswordHash);

        // Act
        var request = new LoginRequest(email, "WrongPassword");
        var response = await _client.SendAsync(HttpMethod.Post, BaseUrl + "login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
