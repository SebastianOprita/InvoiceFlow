namespace InvoiceFlow.Identity.Application;

public interface ITokenService
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateRefreshToken(DateTimeOffset dateTime);
    public string CalculateTokenHash(string token);

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GenerateUserAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        long permissions,
        DateTimeOffset dateTime);

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GeneratePlatformUserAccessToken(
        Guid platformUserId,
        string platformEmail,
        DateTimeOffset dateTime);

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GenerateImpersonationToken(
        Guid platformUserId,
        string platformEmail,
        Guid targetUserId,
        Guid targetTenantId,
        string targetEmail,
        long targetPermissions,
        Guid sessionId,
        string? reason,
        DateTimeOffset dateTime);
}
