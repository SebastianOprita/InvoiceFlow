namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public interface IAccessTokenCache
{
    ValueTask<AccessToken?> GetAsync(
        AccessTokenCacheKey key,
        CancellationToken cancellationToken);

    ValueTask SetAsync(
        AccessTokenCacheKey key,
        AccessToken token,
        DateTimeOffset cacheUntilUtc,
        CancellationToken cancellationToken);
}
