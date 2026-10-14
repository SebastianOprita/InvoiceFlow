namespace InvoiceFlow.Identity.Application;

public interface ITokenService
{
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(DateTime dateTime);
    public string CalculateTokenHash(string token);

    public (string Token, string TokenType, DateTime ExpiresAt) GenerateUserAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        long permissions,
        DateTime dateTime);

    public (string Token, string TokenType, DateTime ExpiresAt) GeneratePlatformUserAccessToken(
        Guid platformUserId,
        string platformEmail,
        DateTime dateTime);

    public (string Token, string TokenType, DateTime ExpiresAt) GenerateImpersonationToken(
        Guid platformUserId,
        string platformEmail,
        Guid targetUserId,
        Guid targetTenantId,
        string targetEmail,
        long targetPermissions,
        Guid sessionId,
        string? reason,
        DateTime dateTime);
}
