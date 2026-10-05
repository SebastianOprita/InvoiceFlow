namespace InvoiceFlow.Identity.Application;

public record LoginPlatformUserCommandResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
