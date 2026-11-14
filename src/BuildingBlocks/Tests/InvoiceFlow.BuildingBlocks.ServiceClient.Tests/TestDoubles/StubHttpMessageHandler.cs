namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    : HttpMessageHandler
{
    private int _callCount;

    public int CallCount => Volatile.Read(ref _callCount);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);

        return handler(request, cancellationToken);
    }
}
