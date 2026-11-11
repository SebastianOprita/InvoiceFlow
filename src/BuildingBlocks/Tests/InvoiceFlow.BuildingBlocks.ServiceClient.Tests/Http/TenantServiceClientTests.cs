using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class TenantServiceClientTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private static TenantServiceClient CreateClient(
        HttpMessageHandler handler,
        IAccessTokenProvider tokenProvider)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://customers.test/")
        };

        return new TenantServiceClient(
            httpClient,
            tokenProvider);
    }

    private static FakeAccessTokenProvider CreateTokenProvider()
    {
        return new FakeAccessTokenProvider
        {
            Token = new AccessToken(
                "test-jwt",
                DateTimeOffset.UtcNow.AddMinutes(15))
        };
    }

    [Fact]
    public async Task SendAsync_WhenSuccessful_ShouldDeserializeResponse()
    {
        // Arrange
        var expectedCustomerId = Guid.NewGuid();

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    id = expectedCustomerId,
                    name = "Test Customer"
                })));

        var tokenProvider = CreateTokenProvider();
        var client = CreateClient(handler, tokenProvider);

        // Act
        var result = await client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            $"customers/{expectedCustomerId:D}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomerId, result.Id);
        Assert.Equal("Test Customer", result.Name);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_ShouldAttachBearerToken()
    {
        // Arrange
        string? capturedScheme = null;
        string? capturedToken = null;

        var handler = new StubHttpMessageHandler(
            (request, _) =>
            {
                capturedScheme =
                    request.Headers.Authorization?.Scheme;

                capturedToken =
                    request.Headers.Authorization?.Parameter;

                return Task.FromResult(
                    JsonResponse(HttpStatusCode.OK, new
                    {
                        id = Guid.NewGuid(),
                        name = "Test Customer"
                    }));
            });

        var tokenProvider = CreateTokenProvider();
        var client = CreateClient(handler, tokenProvider);

        // Act
        await client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            "customers/123",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Bearer", capturedScheme);
        Assert.Equal("test-jwt", capturedToken);
    }

    [Fact]
    public async Task SendAsync_ShouldBuildTenantScopedUrl()
    {
        // Arrange
        Uri? capturedUri = null;

        var handler = new StubHttpMessageHandler(
            (request, _) =>
            {
                capturedUri = request.RequestUri;

                return Task.FromResult(
                    JsonResponse(HttpStatusCode.OK, new
                    {
                        id = Guid.NewGuid(),
                        name = "Test Customer"
                    }));
            });

        var client = CreateClient(
            handler,
            CreateTokenProvider());

        // Act
        await client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            "customers/CUST-001",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(
            $"https://customers.test/api/{_tenantId:D}/customers/CUST-001",
            capturedUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SendAsync_ShouldPassTenantIdToTokenProvider()
    {
        // Arrange
        var tokenProvider = CreateTokenProvider();

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                JsonResponse(HttpStatusCode.OK, new
                {
                    id = Guid.NewGuid(),
                    name = "Test Customer"
                })));

        var client = CreateClient(handler, tokenProvider);

        // Act
        await client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            "customers/123",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_tenantId, tokenProvider.LastTenantId);
        Assert.Equal(1, tokenProvider.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenNoContent_ShouldReturnDefault()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(
                    HttpStatusCode.NoContent)));

        var client = CreateClient(
            handler,
            CreateTokenProvider());

        // Act
        var result = await client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            "customers/123",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task SendAsync_WhenHttpRequestFails_ShouldThrowWithoutRetry(
        HttpStatusCode statusCode)
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(statusCode)));

        var tokenProvider = CreateTokenProvider();

        var client = CreateClient(handler, tokenProvider);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SendAsync<CustomerResponse>(
                HttpMethod.Get,
                _tenantId,
                "customers/123",
                cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(1, tokenProvider.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/customers/123")]
    [InlineData("https://evil.example.com/customers")]
    [InlineData("../customers")]
    [InlineData("customers/../123")]
    [InlineData(@"customers\123")]
    public async Task SendAsync_WhenRelativePathInvalid_ShouldThrow(
        string relativePath)
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)));

        var tokenProvider = CreateTokenProvider();

        var client = CreateClient(handler, tokenProvider);

        // Act
        var action = () => client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            _tenantId,
            relativePath,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal(0, handler.CallCount);
        Assert.Equal(0, tokenProvider.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTenantIdEmpty_ShouldThrow()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)));

        var tokenProvider = CreateTokenProvider();

        var client = CreateClient(handler, tokenProvider);

        // Act
        var action = () => client.SendAsync<CustomerResponse>(
            HttpMethod.Get,
            Guid.Empty,
            "customers/123",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal(0, handler.CallCount);
        Assert.Equal(0, tokenProvider.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenPost_ShouldSendContent()
    {
        // Arrange
        HttpMethod? capturedMethod = null;
        string? capturedBody = null;
        string? capturedContentType = null;

        var handler = new StubHttpMessageHandler(
            async (request, cancellationToken) =>
            {
                capturedMethod = request.Method;

                capturedBody = await request.Content!
                    .ReadAsStringAsync(cancellationToken);

                capturedContentType =
                    request.Content.Headers.ContentType?.MediaType;

                return JsonResponse(HttpStatusCode.OK, new
                {
                    id = Guid.NewGuid(),
                    name = "Created Customer"
                });
            });

        var client = CreateClient(
            handler,
            CreateTokenProvider());

        using var content = JsonContent.Create(new
        {
            name = "Created Customer"
        });

        // Act
        var result = await client.SendAsync<CustomerResponse>(
            HttpMethod.Post,
            _tenantId,
            "customers",
            content,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("application/json", capturedContentType);
        Assert.Contains("Created Customer", capturedBody);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTokenAcquisitionFails_ShouldNotSendHttpRequest()
    {
        // Arrange
        var tokenProvider = CreateTokenProvider();

        tokenProvider.Exception = new HttpRequestException(
            "Identity API unavailable");

        var handler = new StubHttpMessageHandler(
            (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)));

        var client = CreateClient(handler, tokenProvider);

        // Act
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SendAsync<CustomerResponse>(
                HttpMethod.Get,
                _tenantId,
                "customers/123",
                cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(0, handler.CallCount);
        Assert.Equal(1, tokenProvider.CallCount);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_ShouldPropagateCancellation()
    {
        // Arrange
        var tokenProvider = CreateTokenProvider();

        var handler = new StubHttpMessageHandler(
            (_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));

        var client = CreateClient(handler, tokenProvider);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.SendAsync<CustomerResponse>(
                HttpMethod.Get,
                _tenantId,
                "customers/123",
                cancellationToken: cts.Token));
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

    private sealed record CustomerResponse(
        Guid Id,
        string Name);

    private sealed class FakeAccessTokenProvider
        : IAccessTokenProvider
    {
        private int _callCount;

        public AccessToken Token { get; set; } = new(
            "test-jwt",
            DateTimeOffset.UtcNow.AddMinutes(15));

        public Exception? Exception { get; set; }

        public Guid? LastTenantId { get; private set; }

        public int CallCount =>
            Volatile.Read(ref _callCount);

        public Task<AccessToken> GetAsync(
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);

            LastTenantId = tenantId;

            cancellationToken.ThrowIfCancellationRequested();

            if (Exception is not null)
                return Task.FromException<AccessToken>(Exception);

            return Task.FromResult(Token);
        }
    }
}
