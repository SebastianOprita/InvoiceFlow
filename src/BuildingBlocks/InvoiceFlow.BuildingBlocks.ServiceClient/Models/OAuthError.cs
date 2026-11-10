using System.Text.Json.Serialization;

namespace InvoiceFlow.BuildingBlocks.ServiceClient;

internal sealed record OAuthError(
    [property: JsonPropertyName("error")]
    string? Error,

    [property: JsonPropertyName("error_description")]
    string? Description);

public static class OAuthErrorCode
{
    public const string InvalidClient = "invalid_client";
    public const string InvalidRequest = "invalid_request";
    public const string UnauthorizedClient = "unauthorized_client";
    public const string UnsupportedGrantType = "unsupported_grant_type";
    public const string ServerError = "server_error";
}
