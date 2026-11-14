using System.Net;
using System.Net.Http.Json;
using System.Text;
using InvoiceFlow.BuildingBlocks.ServiceClient.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class HttpTokenEndpointClientTests
{
    private readonly FakeTimeProvider _time = new(
        new DateTimeOffset(
            2026, 11, 11, 12, 0, 0,
            TimeSpan.Zero));

    private readonly Guid _tenantId = Guid.NewGuid();

    private static ServiceTokenOptions CreateOptions()
    {
        return new ServiceTokenOptions
        {
            TokenEndpoint = "https://identity.test/connect/token",
            ClientId = "invoices-service",
            ClientSecret = "test-secret",
            GrantType = "client_credentials",
            RefreshSkew = TimeSpan.FromSeconds(30),
            RequestTimeout = TimeSpan.FromSeconds(15)
        };
    }

    private HttpTokenEndpointClient CreateClient(
        HttpMessageHandler handler,
        ServiceTokenOptions? options = null)
    {
        var services = new ServiceCollection();

        services.AddHttpClient(
                HttpTokenEndpointClient.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        var provider = services.BuildServiceProvider();

        return new HttpTokenEndpointClient(
            provider.GetRequiredService<IHttpClientFactory>(),
            Options.Create(options ?? CreateOptions()),
            _time);
    }

    private static HttpResponseMessage JsonResponse(
        HttpStatusCode statusCode,
        object body)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(body)
        };
    }

    private static HttpResponseMessage RawResponse(
        HttpStatusCode statusCode,
        string body,
        string contentType = "application/json")
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                body,
                Encoding.UTF8,
                contentType)
        };
    }

    [Fact]
    public async Task RequestAsync_WhenTenantIdIsEmpty_ShouldThrow()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new { })));

        var client = CreateClient(handler);

        // Act
        var action = () => client.RequestAsync(
            Guid.Empty,
            TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task RequestAsync_WhenSuccessful_ShouldReturnAccessToken()
    {
        // Arrange
        var expectedExpiry = _time.GetUtcNow().AddMinutes(15);

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "jwt-123",
                    token_type = "Bearer",
                    expires_in = 900,
                    expires_at_utc = expectedExpiry
                })));

        var client = CreateClient(handler);

        // Act
        var token = await client.RequestAsync(
            _tenantId,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("jwt-123", token.Value);
        Assert.Equal(expectedExpiry, token.ExpiresAtUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task RequestAsync_ShouldSendCorrectOAuthForm()
    {
        // Arrange
        HttpMethod? capturedMethod = null;
        Uri? capturedUri = null;
        string? capturedContentType = null;
        string? capturedBody = null;

        var handler = new StubHttpMessageHandler(
            async (request, cancellationToken) =>
            {
                capturedMethod = request.Method;
                capturedUri = request.RequestUri;

                capturedContentType =
                    request.Content?.Headers.ContentType?.MediaType;

                capturedBody = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync(
                        cancellationToken);

                return JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "jwt-123",
                    token_type = "Bearer",
                    expires_in = 900
                });
            });

        var client = CreateClient(handler);

        // Act
        await client.RequestAsync(
            _tenantId,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpMethod.Post, capturedMethod);

        Assert.Equal(
            "https://identity.test/connect/token",
            capturedUri?.AbsoluteUri);

        Assert.Equal(
            "application/x-www-form-urlencoded",
            capturedContentType);

        Assert.NotNull(capturedBody);

        var parameters = ParseForm(capturedBody);

        Assert.Equal(4, parameters.Count);
        Assert.Equal("client_credentials", parameters["grant_type"]);
        Assert.Equal("invoices-service", parameters["client_id"]);
        Assert.Equal("test-secret", parameters["client_secret"]);
        Assert.Equal(_tenantId.ToString("D"), parameters["tenant_id"]);
    }

    [Fact]
    public async Task RequestAsync_WhenAbsoluteExpiryIsEarlier_ShouldUseAbsoluteExpiry()
    {
        // Arrange
        var absoluteExpiry = _time.GetUtcNow().AddMinutes(10);

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "jwt-123",
                    token_type = "Bearer",
                    expires_in = 900,
                    expires_at_utc = absoluteExpiry
                })));

        var client = CreateClient(handler);

        // Act
        var token = await client.RequestAsync(
            _tenantId,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(absoluteExpiry, token.ExpiresAtUtc);
    }

    [Fact]
    public async Task RequestAsync_WhenDurationExpiryIsEarlier_ShouldUseDurationExpiry()
    {
        // Arrange
        var absoluteExpiry = _time.GetUtcNow().AddMinutes(30);

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "jwt-123",
                    token_type = "Bearer",
                    expires_in = 900,
                    expires_at_utc = absoluteExpiry
                })));

        var client = CreateClient(handler);

        // Act
        var token = await client.RequestAsync(
            _tenantId,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            _time.GetUtcNow().AddMinutes(15),
            token.ExpiresAtUtc);
    }

    [Fact]
    public async Task RequestAsync_WhenAbsoluteExpiryMissing_ShouldUseExpiresIn()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "jwt-123",
                    token_type = "Bearer",
                    expires_in = 600
                })));

        var client = CreateClient(handler);

        // Act
        var token = await client.RequestAsync(
            _tenantId,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            _time.GetUtcNow().AddMinutes(10),
            token.ExpiresAtUtc);
    }

    [Theory]
    [InlineData("invalid_client", HttpStatusCode.Unauthorized)]
    [InlineData("invalid_request", HttpStatusCode.BadRequest)]
    [InlineData("unauthorized_client", HttpStatusCode.BadRequest)]
    [InlineData("unsupported_grant_type", HttpStatusCode.BadRequest)]
    [InlineData("server_error", HttpStatusCode.InternalServerError)]
    public async Task RequestAsync_WhenOAuthError_ShouldThrowCorrectError(
        string errorCode,
        HttpStatusCode statusCode)
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(statusCode, new
                {
                    error = errorCode
                })));

        var client = CreateClient(handler);

        // Act
        var exception =
            await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
                () => client.RequestAsync(
                    _tenantId,
                    TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(errorCode, exception.OAuthError);
    }

    [Fact]
    public async Task RequestAsync_WhenUnknownOAuthError_ShouldPreserveErrorCode()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.BadRequest, new
                {
                    error = "custom_oauth_error"
                })));

        var client = CreateClient(handler);

        // Act
        var exception =
            await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
                () => client.RequestAsync(
                    _tenantId,
                    TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(
            "custom_oauth_error",
            exception.OAuthError);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            exception.StatusCode);
    }

    [Fact]
    public async Task RequestAsync_WhenOAuthErrorBodyIsMalformed_ShouldPreserveHttpFailure()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                RawResponse(
                    HttpStatusCode.InternalServerError,
                    "{invalid-json")));

        var client = CreateClient(handler);

        // Act
        var exception =
            await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
                () => client.RequestAsync(
                    _tenantId,
                    TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            exception.StatusCode);

        Assert.Null(exception.OAuthError);
    }

    [Fact]
    public async Task RequestAsync_WhenErrorResponseIsHtml_ShouldPreserveHttpFailure()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                RawResponse(
                    HttpStatusCode.BadGateway,
                    "<html>Bad Gateway</html>",
                    "text/html")));

        var client = CreateClient(handler);

        // Act
        var exception =
            await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
                () => client.RequestAsync(
                    _tenantId,
                    TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(
            HttpStatusCode.BadGateway,
            exception.StatusCode);

        Assert.Null(exception.OAuthError);
    }

    [Theory]
    [InlineData(null, "Bearer", 900)]
    [InlineData("", "Bearer", 900)]
    [InlineData("jwt-123", "Basic", 900)]
    [InlineData("jwt-123", "Bearer", 0)]
    [InlineData("jwt-123", "Bearer", -1)]
    public async Task RequestAsync_WhenTokenResponseInvalid_ShouldThrow(
        string? accessToken,
        string tokenType,
        int expiresIn)
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = accessToken,
                    token_type = tokenType,
                    expires_in = expiresIn
                })));

        var client = CreateClient(handler);

        // Act & Assert
        await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
            () => client.RequestAsync(
                _tenantId,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestAsync_WhenSuccessJsonMalformed_ShouldThrow()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                RawResponse(
                    HttpStatusCode.OK,
                    "{invalid-json")));

        var client = CreateClient(handler);

        // Act
        var exception =
            await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
                () => client.RequestAsync(
                    _tenantId,
                    TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(HttpStatusCode.OK, exception.StatusCode);
    }

    [Fact]
    public async Task RequestAsync_WhenSuccessContentTypeUnsupported_ShouldThrow()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                RawResponse(
                    HttpStatusCode.OK,
                    "<html>unexpected</html>",
                    "text/html")));

        var client = CreateClient(handler);

        // Act
        await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
            () => client.RequestAsync(
                _tenantId,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestAsync_WhenHttpRequestFails_ShouldPropagate()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => throw new HttpRequestException(
                "Connection failed"));

        var client = CreateClient(handler);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.RequestAsync(
                _tenantId,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestAsync_WhenCancelled_ShouldPropagateCancellation()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));

        var client = CreateClient(handler);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.RequestAsync(_tenantId, cts.Token));
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        return body.Split('&')
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                parts => Uri.UnescapeDataString(
                    parts[0].Replace("+", " ")),
                parts => Uri.UnescapeDataString(
                    parts[1].Replace("+", " ")));
    }
}
