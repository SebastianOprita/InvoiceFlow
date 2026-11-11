namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

internal sealed class FakeTokenEndpointClient : ITokenEndpointClient
{
    private int _callCount;

    public int CallCount => Volatile.Read(ref _callCount);

    public Func<Guid, CancellationToken, Task<AccessToken>> Handler { get; set; }
        = (_, _) => throw new NotImplementedException();

    public async Task<AccessToken> RequestAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);

        return await Handler(tenantId, cancellationToken);
    }
}
