using System.Text.Json.Serialization;

namespace InvoiceFlow.Identity.Api;

public record LoginRequest(
    [property: JsonRequired] string Email,
    [property: JsonRequired] string Password);
public record RefreshTokenRequest(
    [property: JsonRequired] string RefreshToken);
public record ChangePasswordRequest(
    [property: JsonRequired] string CurrentPassword,
    [property: JsonRequired] string NewPassword);
