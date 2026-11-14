using System.Net.Http.Headers;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

internal sealed class TestClient
{
    private readonly HttpClient _client;

    public TestClient(PaymentsApiFactory factory) => _client = factory.CreateClient();

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request, cancellationToken);
    }
}
