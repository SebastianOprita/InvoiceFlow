namespace InvoiceFlow.Identity.Application;

public record RefreshTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
