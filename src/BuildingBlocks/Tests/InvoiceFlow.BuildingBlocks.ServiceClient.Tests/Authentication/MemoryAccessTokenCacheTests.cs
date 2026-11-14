using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class MemoryAccessTokenCacheTests : IDisposable
{
    private readonly FakeTimeProvider _time = new(
        new DateTimeOffset(
            2026, 11, 11, 12, 0, 0,
            TimeSpan.Zero));

    private readonly MemoryCache _memoryCache;

    private readonly MemoryAccessTokenCache _cache;

    public MemoryAccessTokenCacheTests()
    {
        _memoryCache = new MemoryCache(
            new MemoryCacheOptions
            {
                Clock = new FakeSystemClock(_time)
            });

        _cache = new MemoryAccessTokenCache(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = CreateKey();

        // Act
        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WhenTokenIsValid_ShouldStoreToken()
    {
        // Arrange
        var key = CreateKey();

        var token = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        var cacheUntilUtc =
            _time.GetUtcNow().AddMinutes(14);

        // Act
        await _cache.SetAsync(
            key,
            token,
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result);
        Assert.Equal("jwt-1", result.Value);
        Assert.Equal(token.ExpiresAtUtc, result.ExpiresAtUtc);
    }

    [Fact]
    public async Task GetAsync_WhenCacheEntryHasNotExpired_ShouldReturnToken()
    {
        // Arrange
        var key = CreateKey();

        var token = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        await _cache.SetAsync(
            key,
            token,
            _time.GetUtcNow().AddMinutes(14),
            TestContext.Current.CancellationToken);

        // Act
        _time.Advance(TimeSpan.FromMinutes(13));

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(token, result);
    }

    [Fact]
    public async Task GetAsync_WhenCacheUntilUtcReached_ShouldReturnNull()
    {
        // Arrange
        var key = CreateKey();

        var token = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        var cacheUntilUtc =
            _time.GetUtcNow().AddMinutes(14);

        await _cache.SetAsync(
            key,
            token,
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        // Act
        _time.Advance(TimeSpan.FromMinutes(14));

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenCacheExpiresBeforeToken_ShouldReturnNull()
    {
        // Arrange
        var key = CreateKey();

        var token = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        await _cache.SetAsync(
            key,
            token,
            _time.GetUtcNow().AddMinutes(14),
            TestContext.Current.CancellationToken);

        // Act
        _time.Advance(
            TimeSpan.FromMinutes(14) +
            TimeSpan.FromSeconds(30));

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);

        // The JWT itself has not expired yet.
        Assert.True(token.ExpiresAtUtc > _time.GetUtcNow());
    }

    [Fact]
    public async Task SetAsync_WhenSameKeyIsUsed_ShouldReplaceExistingToken()
    {
        // Arrange
        var key = CreateKey();

        var firstToken = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        var secondToken = new AccessToken(
            "jwt-2",
            _time.GetUtcNow().AddMinutes(20));

        // Act
        await _cache.SetAsync(
            key,
            firstToken,
            _time.GetUtcNow().AddMinutes(14),
            TestContext.Current.CancellationToken);

        await _cache.SetAsync(
            key,
            secondToken,
            _time.GetUtcNow().AddMinutes(19),
            TestContext.Current.CancellationToken);

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt-2", result.Value);
        Assert.Equal(secondToken.ExpiresAtUtc, result.ExpiresAtUtc);
    }

    [Fact]
    public async Task GetAsync_ForDifferentTenants_ShouldReturnSeparateTokens()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var keyA = new AccessTokenCacheKey(
            "invoices-service",
            tenantA);

        var keyB = new AccessTokenCacheKey(
            "invoices-service",
            tenantB);

        var tokenA = new AccessToken(
            "jwt-tenant-a",
            _time.GetUtcNow().AddMinutes(15));

        var tokenB = new AccessToken(
            "jwt-tenant-b",
            _time.GetUtcNow().AddMinutes(15));

        var cacheUntilUtc =
            _time.GetUtcNow().AddMinutes(14);

        await _cache.SetAsync(
            keyA,
            tokenA,
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        await _cache.SetAsync(
            keyB,
            tokenB,
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        // Act
        var resultA = await _cache.GetAsync(
            keyA,
            TestContext.Current.CancellationToken);

        var resultB = await _cache.GetAsync(
            keyB,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("jwt-tenant-a", resultA?.Value);
        Assert.Equal("jwt-tenant-b", resultB?.Value);
    }

    [Fact]
    public async Task GetAsync_ForDifferentClientIds_ShouldReturnSeparateTokens()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var keyA = new AccessTokenCacheKey(
            "invoices-service",
            tenantId);

        var keyB = new AccessTokenCacheKey(
            "payments-service",
            tenantId);

        var cacheUntilUtc =
            _time.GetUtcNow().AddMinutes(14);

        await _cache.SetAsync(
            keyA,
            new AccessToken(
                "jwt-invoices",
                _time.GetUtcNow().AddMinutes(15)),
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        await _cache.SetAsync(
            keyB,
            new AccessToken(
                "jwt-payments",
                _time.GetUtcNow().AddMinutes(15)),
            cacheUntilUtc,
            TestContext.Current.CancellationToken);

        // Act
        var resultA = await _cache.GetAsync(
            keyA,
            TestContext.Current.CancellationToken);

        var resultB = await _cache.GetAsync(
            keyB,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("jwt-invoices", resultA?.Value);
        Assert.Equal("jwt-payments", resultB?.Value);
    }

    [Fact]
    public async Task GetAsync_WhenCancelled_ShouldThrow()
    {
        // Arrange
        var key = CreateKey();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var action = async () => await _cache.GetAsync(
            key,
            cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(action);
    }

    [Fact]
    public async Task SetAsync_WhenCancelled_ShouldThrowAndNotStoreToken()
    {
        // Arrange
        var key = CreateKey();

        var token = new AccessToken(
            "jwt-1",
            _time.GetUtcNow().AddMinutes(15));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var action = async () => await _cache.SetAsync(
            key,
            token,
            _time.GetUtcNow().AddMinutes(14),
            cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(action);

        var result = await _cache.GetAsync(
            key,
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    private static AccessTokenCacheKey CreateKey()
    {
        return new AccessTokenCacheKey(
            "invoices-service",
            Guid.NewGuid());
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    private sealed class FakeSystemClock(TimeProvider time)
        : Microsoft.Extensions.Internal.ISystemClock
    {
        public DateTimeOffset UtcNow => time.GetUtcNow();
    }
}
