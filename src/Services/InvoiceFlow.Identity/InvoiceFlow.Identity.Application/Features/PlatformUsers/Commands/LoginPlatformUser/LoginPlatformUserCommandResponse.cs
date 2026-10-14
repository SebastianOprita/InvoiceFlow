namespace InvoiceFlow.Identity.Application;

public sealed record LoginPlatformUserCommandResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
