using System.Collections.Concurrent;
using InvoiceFlow.BuildingBlocks.ServiceClient.Exceptions;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed class AccessTokenProvider(
    IAccessTokenCache cache,
    ITokenEndpointClient tokenEndpointClient,
    IOptions<ServiceTokenOptions> options,
    TimeProvider timeProvider)
    : IAccessTokenProvider
{
    private readonly ServiceTokenOptions _options =
        options.Value;

    private readonly ConcurrentDictionary<
        AccessTokenCacheKey,
        Lazy<Task<AccessToken>>> _inflight = new();

    public async Task<AccessToken> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));

        cancellationToken.ThrowIfCancellationRequested();

        var key = new AccessTokenCacheKey(
            _options.ClientId,
            tenantId);

        var cached = await cache.GetAsync(
            key,
            cancellationToken);

        if (cached is not null)
            return cached;

        // Create a candidate acquisition.
        var candidate = new Lazy<Task<AccessToken>>(
            () => RequestAndCacheAsync(key, tenantId),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // Only one Lazy is stored per key.
        var flight = _inflight.GetOrAdd(key, candidate);

        // All callers share the same task.
        var sharedTask = flight.Value;

        // The owner registers cleanup.
        if (ReferenceEquals(flight, candidate))
        {
            _ = sharedTask.ContinueWith(
                completed =>
                {
                    if (completed.IsFaulted)
                        _ = completed.Exception;

                    RemoveFlight(key, flight);

                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        // Caller cancellation does not cancel shared acquisition.
        return await sharedTask.WaitAsync(cancellationToken);
    }

    private async Task<AccessToken> RequestAndCacheAsync(
        AccessTokenCacheKey key,
        Guid tenantId)
    {
        // Another request may have populated the cache.
        var cached = await cache.GetAsync(
            key,
            CancellationToken.None);

        if (cached is not null)
            return cached;

        // Shared acquisition is independent of caller cancellation.
        var token = await tokenEndpointClient.RequestAsync(
            tenantId,
            CancellationToken.None);

        var cacheUntilUtc =
            token.ExpiresAtUtc - _options.RefreshSkew;

        if (cacheUntilUtc <= timeProvider.GetUtcNow())
        {
            throw new ServiceClientAuthenticationException(
                "The token expires before its refresh threshold.");
        }

        await cache.SetAsync(
            key,
            token,
            cacheUntilUtc,
            CancellationToken.None);

        return token;
    }

    private void RemoveFlight(
        AccessTokenCacheKey key,
        Lazy<Task<AccessToken>> flight)
    {
        // Remove only if the dictionary still contains
        // this exact Lazy instance.
        ((ICollection<KeyValuePair<
            AccessTokenCacheKey,
            Lazy<Task<AccessToken>>>>)_inflight)
            .Remove(new KeyValuePair<
                AccessTokenCacheKey,
                Lazy<Task<AccessToken>>>(key, flight));
    }
}
