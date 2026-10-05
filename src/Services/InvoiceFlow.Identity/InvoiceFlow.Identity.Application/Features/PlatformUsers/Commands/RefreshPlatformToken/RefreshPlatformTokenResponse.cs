namespace InvoiceFlow.Identity.Application;

public record RefreshPlatformTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
