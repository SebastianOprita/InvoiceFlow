using System.Net.Http.Json;
using System.Text.Json.Serialization;
using InvoiceFlow.BuildingBlocks.ServiceClient.Exceptions;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

public sealed class HttpTokenEndpointClient(
    IHttpClientFactory httpClientFactory,
    IOptions<ServiceTokenOptions> options,
    TimeProvider timeProvider)
    : ITokenEndpointClient
{
    public const string HttpClientName =
        "InvoiceFlow.ServiceClient.TokenEndpoint";

    private readonly ServiceTokenOptions _options = options.Value;

    public async Task<AccessToken> RequestAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = _options.GrantType,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["tenant_id"] = tenantId.ToString("D")
        };

        using var content =
            new FormUrlEncodedContent(parameters);

        var client = httpClientFactory.CreateClient(
            HttpClientName);

        var requestedAt = timeProvider.GetUtcNow();

        using var response = await client.PostAsync(
            _options.TokenEndpoint,
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceClientAuthenticationException(
                "OAuth token acquisition failed.",
                response.StatusCode);
        }

        var body = await response.Content
            .ReadFromJsonAsync<TokenResponse>(
                cancellationToken);

        if (body is null ||
            string.IsNullOrWhiteSpace(body.AccessToken) ||
            !string.Equals(
                body.TokenType,
                "Bearer",
                StringComparison.OrdinalIgnoreCase) ||
            body.ExpiresIn is null or <= 0)
        {
            throw new ServiceClientAuthenticationException(
                "The token endpoint returned an invalid response.");
        }

        var expiresFromDuration =
            requestedAt.AddSeconds(body.ExpiresIn.Value);

        var expiresAtUtc = body.ExpiresAtUtc is { } absolute
            ? Min(expiresFromDuration, absolute)
            : expiresFromDuration;

        return new AccessToken(
            body.AccessToken,
            expiresAtUtc);
    }

    private static DateTimeOffset Min(
        DateTimeOffset first,
        DateTimeOffset second)
    {
        return first <= second ? first : second;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")]
        string? AccessToken,

        [property: JsonPropertyName("token_type")]
        string? TokenType,

        [property: JsonPropertyName("expires_in")]
        int? ExpiresIn,

        [property: JsonPropertyName("expires_at_utc")]
        DateTimeOffset? ExpiresAtUtc);

}
