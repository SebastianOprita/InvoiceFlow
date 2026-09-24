namespace InvoiceFlow.Identity.Application;

public record LoginUserCommandResponse(
    string AccessToken,
    string TokenType,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
