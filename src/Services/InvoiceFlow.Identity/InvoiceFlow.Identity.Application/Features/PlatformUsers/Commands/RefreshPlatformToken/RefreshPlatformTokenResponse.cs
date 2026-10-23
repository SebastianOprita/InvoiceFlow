namespace InvoiceFlow.Identity.Application;

public sealed record RefreshPlatformTokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
