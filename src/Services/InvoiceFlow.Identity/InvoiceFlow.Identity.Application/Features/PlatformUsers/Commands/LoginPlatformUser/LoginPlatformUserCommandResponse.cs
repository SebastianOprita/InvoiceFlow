namespace InvoiceFlow.Identity.Application;

public sealed record LoginPlatformUserCommandResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email);
