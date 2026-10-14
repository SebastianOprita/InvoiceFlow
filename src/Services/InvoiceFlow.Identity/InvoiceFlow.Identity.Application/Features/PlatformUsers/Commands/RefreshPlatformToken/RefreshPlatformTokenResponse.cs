namespace InvoiceFlow.Identity.Application;

public sealed record RefreshPlatformTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
