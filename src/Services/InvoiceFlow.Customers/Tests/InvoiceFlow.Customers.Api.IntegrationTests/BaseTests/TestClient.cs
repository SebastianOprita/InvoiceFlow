using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

internal sealed class TestClient
{
    private readonly HttpClient _client;
    public TestClient(CustomersApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string? token = null)
    {
        using var request = new HttpRequestMessage(method, url);

        if (token is not null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(
            request,
            TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync<T>(
        HttpMethod method,
        string url,
        T? content = default,
        string? token = null)
    {
        using var request = new HttpRequestMessage(method, url);

        if (token is not null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        if (content is not null)
        {
            request.Content = JsonContent.Create(content);
        }

        return await _client.SendAsync(
            request,
            TestContext.Current.CancellationToken);
    }
}
