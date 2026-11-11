using InvoiceFlow.BuildingBlocks.ServiceClient.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class AccessTokenProviderTests
{
    private readonly FakeTimeProvider _time = new(
        new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero));

    private readonly FakeTokenEndpointClient _endpoint = new();

    private AccessTokenProvider CreateProvider()
    {
        var options = Options.Create(new ServiceTokenOptions
        {
            TokenEndpoint = "https://identity.test/connect/token",
            ClientId = "invoices-service",
            ClientSecret = "test-secret",
            GrantType = "client_credentials",
            RefreshSkew = TimeSpan.FromSeconds(30),
            RequestTimeout = TimeSpan.FromSeconds(15)
        });

        return new AccessTokenProvider(
            new MemoryAccessTokenCache(
                new MemoryCache(new MemoryCacheOptions
                {
                    Clock = new FakeSystemClock(_time)
                })),
            _endpoint,
            options,
            _time);
    }

    private sealed class FakeSystemClock(TimeProvider time)
        : Microsoft.Extensions.Internal.ISystemClock
    {
        public DateTimeOffset UtcNow => time.GetUtcNow();
    }


    [Fact]
    public async Task GetAsync_WhenTenantIdIsEmpty_ShouldThrow()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var action = () => provider.GetAsync(Guid.Empty);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        Assert.Equal(0, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsEmpty_ShouldRequestToken()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _endpoint.Handler = (_, _) =>
            Task.FromResult(new AccessToken(
                "jwt-1",
                _time.GetUtcNow().AddMinutes(15)));

        var provider = CreateProvider();

        // Act
        var token = await provider.GetAsync(tenantId);

        // Assert
        Assert.Equal("jwt-1", token.Value);
        Assert.Equal(1, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenTokenIsCached_ShouldNotRequestAgain()
    {
        var tenantId = Guid.NewGuid();

        _endpoint.Handler = (_, _) =>
            Task.FromResult(new AccessToken(
                "jwt-1",
                _time.GetUtcNow().AddMinutes(15)));

        var provider = CreateProvider();

        var first = await provider.GetAsync(tenantId);
        var second = await provider.GetAsync(tenantId);

        Assert.Equal(first.Value, second.Value);
        Assert.Equal(1, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_ForDifferentTenants_ShouldAcquireSeparateTokens()
    {
        _endpoint.Handler = (tenantId, _) =>
            Task.FromResult(new AccessToken(
                $"jwt-{tenantId}",
                _time.GetUtcNow().AddMinutes(15)));

        var provider = CreateProvider();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tokenA = await provider.GetAsync(tenantA);
        var tokenB = await provider.GetAsync(tenantB);

        Assert.NotEqual(tokenA.Value, tokenB.Value);
        Assert.Equal(2, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_ConcurrentRequests_ShouldAcquireTokenOnce()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var started = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var release = new TaskCompletionSource<AccessToken>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _endpoint.Handler = async (_, _) =>
        {
            started.TrySetResult();
            return await release.Task;
        };

        var provider = CreateProvider();

        // Act
        var requests = Enumerable.Range(0, 100)
            .Select(_ => provider.GetAsync(tenantId))
            .ToArray();

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        release.SetResult(new AccessToken(
            "shared-jwt",
            _time.GetUtcNow().AddMinutes(15)));

        var results = await Task.WhenAll(requests);

        // Assert
        Assert.Equal(100, results.Length);
        Assert.All(results, token =>
            Assert.Equal("shared-jwt", token.Value));

        Assert.Equal(1, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenCacheExpires_ShouldAcquireNewToken()
    {
        var tenantId = Guid.NewGuid();
        var version = 0;

        _endpoint.Handler = (_, _) =>
        {
            var currentVersion = Interlocked.Increment(ref version);

            return Task.FromResult(new AccessToken(
                $"jwt-{currentVersion}",
                _time.GetUtcNow().AddMinutes(15)));
        };

        var provider = CreateProvider();

        var first = await provider.GetAsync(tenantId);

        _time.Advance(TimeSpan.FromMinutes(14) +
                      TimeSpan.FromSeconds(31));

        var second = await provider.GetAsync(tenantId);

        Assert.Equal("jwt-1", first.Value);
        Assert.Equal("jwt-2", second.Value);
        Assert.Equal(2, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenTokenExpiresBeforeRefreshWindow_ShouldThrow()
    {
        var tenantId = Guid.NewGuid();

        _endpoint.Handler = (_, _) =>
            Task.FromResult(new AccessToken(
                "short-lived-jwt",
                _time.GetUtcNow().AddSeconds(10)));

        var provider = CreateProvider();

        await Assert.ThrowsAsync<ServiceClientAuthenticationException>(
            () => provider.GetAsync(tenantId));
    }

    [Fact]
    public async Task GetAsync_WhenAcquisitionFails_ShouldAllowNextAttempt()
    {
        var tenantId = Guid.NewGuid();

        var calls = 0;

        _endpoint.Handler = (_, _) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
                throw new HttpRequestException("Identity unavailable");

            return Task.FromResult(new AccessToken(
                "recovered-jwt",
                _time.GetUtcNow().AddMinutes(15)));
        };

        var provider = CreateProvider();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetAsync(tenantId));

        var token = await provider.GetAsync(tenantId);

        Assert.Equal("recovered-jwt", token.Value);
        Assert.Equal(2, _endpoint.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenOneCallerCancels_ShouldNotCancelSharedAcquisition()
    {
        var tenantId = Guid.NewGuid();

        var started = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var release = new TaskCompletionSource<AccessToken>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _endpoint.Handler = async (_, _) =>
        {
            started.TrySetResult();
            return await release.Task;
        };

        var provider = CreateProvider();

        using var cts = new CancellationTokenSource();

        var cancelledCaller = provider.GetAsync(tenantId, cts.Token);
        var activeCaller = provider.GetAsync(tenantId);

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => cancelledCaller);

        release.SetResult(new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15)));

        var token = await activeCaller;

        Assert.Equal("jwt-1", token.Value);
        Assert.Equal(1, _endpoint.CallCount);
    }
}
