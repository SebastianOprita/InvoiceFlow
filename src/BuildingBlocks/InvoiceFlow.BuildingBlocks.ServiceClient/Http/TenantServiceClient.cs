using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed class TenantServiceClient(
    HttpClient httpClient,
    IAccessTokenProvider accessTokenProvider)
{
    public async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        Guid tenantId,
        string relativePath,
        HttpContent? content = null,
        bool retryOnUnauthorized = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));

        ValidateRelativePath(relativePath);

        var token = await accessTokenProvider.GetAsync(
            tenantId,
            cancellationToken);

        var path = $"api/{tenantId:D}/{relativePath}";

        using var request = new HttpRequestMessage(method, path)
        {
            Content = content
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token.Value);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        return await response.Content
            .ReadFromJsonAsync<TResponse>(
                cancellationToken);
    }

    private static void ValidateRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.StartsWith('/') ||
            path.Contains('\\') ||
            Uri.TryCreate(path, UriKind.Absolute, out _) ||
            path.Split('/').Any(
                segment => segment is "." or ".."))
        {
            throw new ArgumentException(
                "Path must be a safe relative API path.",
                nameof(path));
        }
    }
}
