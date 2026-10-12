using FluentAssertions;
using InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;
using InvoiceFlow.Identity.Application;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests;

[Collection(PlatformUsersApiCollection.Name)]
public sealed class PlatformUsersControllerTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private readonly TestClient _client;
    private readonly string _baseUrl;
    private readonly string _accessToken;

    public PlatformUsersControllerTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = new TestClient(factory);
        _baseUrl = $"/api/platform/users/";
        _accessToken = TestJwtTokenFactory.CreatePlatformUserAccessToken(_factory.PlatformUser.Id, _factory.JwtSettings);
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
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl, _accessToken, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenUserExists_Should_ReturnOk()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;

        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{platformUser.Id}", _accessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<PlatformUserDto>(TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user!.Id.Should().Be(platformUser.Id);
        user.Email.Should().Be(platformUser.Email.Value);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{Guid.CreateVersion7()}", _accessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Should_Return_Created()
    {
        // Arrange
        string email = "test2@example.com";

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _accessToken, CreateValidUserRequest(email), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var user = await response.Content.ReadFromJsonAsync<PlatformUserDto>(TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task Create_WhenUserEmailAlreadyExists_Should_ReturnConflict()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;

        var request = CreateValidUserRequest(platformUser.Email.Value);

        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl, _accessToken, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.Should().Contain(ApplicationErrors.UserEmailAlreadyExists.Code);
    }

    [Fact]
    public async Task Update_WhenUserExists_Should_ReturnOk()
    {
        // Arrange
        var platformUser = _factory.PlatformUser;
        var updateRequest = new UpdatePlatformUserRequest
            ("FirstName", "LastName");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, _baseUrl + $"{platformUser.Id}", _accessToken, updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await response.Content
            .ReadFromJsonAsync<PlatformUserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser!.Id.Should().Be(platformUser.Id);
        updatedUser.FirstName.Should().Be("FirstName");
        updatedUser.LastName.Should().Be("LastName");
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdatePlatformUserRequest
            ("FirstName", "LastName");

        // Act
        var response = await _client.SendAsync(HttpMethod.Put, _baseUrl + $"{Guid.CreateVersion7()}", _accessToken, updateRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenUserIsInactive_Should_ReturnOk()
    {
        // Arrange
        string email = "test5@example.com";
        var createUserId = await _factory.InsertPlatformUserAsync(email, TestConstants.PasswordHash, null, false);

        // Act
        var activateResponse = await _client.SendAsync(HttpMethod.Post, _baseUrl + $"activate/{createUserId}", _accessToken, TestContext.Current.CancellationToken);

        // Assert
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedResponse = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{createUserId}", _accessToken, TestContext.Current.CancellationToken);

        var updatedUser = await updatedResponse.Content
            .ReadFromJsonAsync<PlatformUserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Post, _baseUrl + $"activate/{Guid.CreateVersion7()}", _accessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenUserIsInactive_Should_ReturnOk()
    {
        // Arrange
        string email = "test5@example.com";
        var createUserId = await _factory.InsertPlatformUserAsync(email, TestConstants.PasswordHash, null, false);

        // Act
        var deactivateResponse = await _client.SendAsync(HttpMethod.Delete, _baseUrl + $"deactivate/{createUserId}", _accessToken, TestContext.Current.CancellationToken);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert
        var updatedResponse = await _client.SendAsync(HttpMethod.Get, _baseUrl + $"{createUserId}", _accessToken, TestContext.Current.CancellationToken);

        var updatedUser = await updatedResponse.Content
            .ReadFromJsonAsync<PlatformUserDto>(TestContext.Current.CancellationToken);

        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenUserDoesNotExist_Should_ReturnNotFound()
    {
        // Act
        var response = await _client.SendAsync(HttpMethod.Delete, _baseUrl + $"deactivate/{Guid.CreateVersion7()}", _accessToken, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> AuthorizedEndpoints =>
    [
        [HttpMethod.Get,    "/api/platform/users"],
        [HttpMethod.Get,    "/api/platform/users/{userId}"],
        [HttpMethod.Post,   "/api/platform/users"],
        [HttpMethod.Put,    "/api/platform/users/{userId}"],
        [HttpMethod.Post,   "/api/platform/users/activate/{userId}"],
        [HttpMethod.Delete, "/api/platform/users/deactivate/{userId}"],
    ];

    [Theory]
    [MemberData(nameof(AuthorizedEndpoints))]
    public async Task Endpoints_Should_Return_Unauthorized_When_Token_Is_Empty(
        HttpMethod method,
        string url)
    {
        url = url.Replace("{userId}", Guid.CreateVersion7().ToString());

        var response = await _client.SendAsync(method, url, "", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static CreatePlatformUserRequest CreateValidUserRequest(string email)
    {
        return new CreatePlatformUserRequest
        (
            email,
            "Password123!",
            "John",
            "Doe"
        );
    }
}
