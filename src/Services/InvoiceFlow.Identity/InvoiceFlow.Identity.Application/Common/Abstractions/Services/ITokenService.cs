using InvoiceFlow.BuildingBlocks.Authorization.Permissions;

namespace InvoiceFlow.Identity.Application;

public interface ITokenService
{
    string GenerateRefreshToken();
    string CalculateTokenHash(string token);

    public (string Token, string TokenType, DateTime ExpiresAt) GenerateUserAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        SystemPermission permissions,
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
        SystemPermission targetPermissions,
        Guid sessionId,
        string? reason,
        DateTime dateTime);
}
