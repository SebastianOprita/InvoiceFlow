using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

public record LoginRequest(
    [property: JsonRequired] string Email,
    [property: JsonRequired] string Password);
public record RefreshTokenRequest(
    [property: JsonRequired] string RefreshToken);
public record ChangePasswordRequest(
    [property: JsonRequired] string CurrentPassword,
    [property: JsonRequired] string NewPassword);

public sealed record ClientCredentialsTokenRequest(
    [FromForm(Name = "grant_type")]
    [property: JsonRequired]
    string GrantType,

    [FromForm(Name = "client_id")]
    [property: JsonRequired]
    string ClientId,

    [FromForm(Name = "client_secret")]
    [property: JsonRequired]
    string ClientSecret,

    [FromForm(Name = "tenant_id")]
    [property: JsonRequired]
    Guid TenantId);
