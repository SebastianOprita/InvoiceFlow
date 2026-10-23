namespace InvoiceFlow.Identity.Application;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
