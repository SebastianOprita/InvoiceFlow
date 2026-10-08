namespace InvoiceFlow.Identity.Application;

public record RefreshTokenResponse(
    string AccessToken,
    string TokenType,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
