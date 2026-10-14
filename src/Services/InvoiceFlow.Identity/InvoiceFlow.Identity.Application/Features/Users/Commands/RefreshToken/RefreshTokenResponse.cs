namespace InvoiceFlow.Identity.Application;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string TokenType,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
