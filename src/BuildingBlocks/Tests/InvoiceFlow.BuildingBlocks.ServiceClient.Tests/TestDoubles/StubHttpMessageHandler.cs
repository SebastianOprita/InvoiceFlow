namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> handler)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(handler(request));
    }
}
