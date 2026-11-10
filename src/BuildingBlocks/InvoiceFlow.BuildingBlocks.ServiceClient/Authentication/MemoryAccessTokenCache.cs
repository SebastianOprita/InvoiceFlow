using Microsoft.Extensions.Caching.Memory;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed class MemoryAccessTokenCache(
    IMemoryCache cache)
    : IAccessTokenCache
{
    public ValueTask<AccessToken?> GetAsync(
        AccessTokenCacheKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cache.TryGetValue(
            key,
            out AccessToken? token);

        return ValueTask.FromResult(token);
    }

    public ValueTask SetAsync(
        AccessTokenCacheKey key,
        AccessToken token,
        DateTimeOffset cacheUntilUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cache.Set(
            key,
            token,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = cacheUntilUtc
            });

        return ValueTask.CompletedTask;
    }
}
