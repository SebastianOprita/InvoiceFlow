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
    string? GrantType,

    [FromForm(Name = "client_id")]
    string? ClientId,

    [FromForm(Name = "client_secret")]
    string? ClientSecret,

    [FromForm(Name = "tenant_id")]
    Guid TenantId);
