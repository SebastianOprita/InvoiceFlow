using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;

internal sealed class TestClient
{
    private readonly HttpClient _client;

    public TestClient(InvoicesApiFactory factory) => _client = factory.CreateClient();

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string? token,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _client.SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync<T>(
        HttpMethod method,
        string url,
        string? token,
        T? content,
        CancellationToken cancellationToken,
        string? idempotencyKey = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        if (content is not null)
            request.Content = JsonContent.Create(content);

        return await _client.SendAsync(request, cancellationToken);
    }
}
