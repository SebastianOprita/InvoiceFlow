using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Application;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceFlow.Identity.Infrastructure;

public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public string CalculateTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public (string Token, string TokenType, DateTime ExpiresAt) GenerateUserAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        SystemPermission permissions,
        DateTime dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, userId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, email),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, ((long)permissions).ToString())
        };

        return (GenerateToken(claims, dateTime, expiresAt), "Bearer", expiresAt);
    }

    public (string Token, string TokenType, DateTime ExpiresAt) GeneratePlatformUserAccessToken(
        Guid platformUserId,
        string platformEmail,
        DateTime dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.PlatformUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, platformUserId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, platformEmail),
        };

        return (GenerateToken(claims, dateTime, expiresAt), "Bearer", expiresAt);
    }

    public (string Token, string TokenType, DateTime ExpiresAt) GenerateImpersonationToken(
        Guid platformUserId,
        string platformEmail,
        Guid targetUserId,
        Guid targetTenantId,
        string targetEmail,
        SystemPermission targetPermissions,
        Guid sessionId,
        string? reason,
        DateTime dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.ImpersonationTokenMinutes);

        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Impersonation),
            new(InvoiceFlowClaimTypes.UserId, targetUserId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, targetEmail),
            new(InvoiceFlowClaimTypes.TenantId, targetTenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, ((long)targetPermissions).ToString()),

            // The actor performing the impersonation
            new(InvoiceFlowClaimTypes.ActorPrincipalType, PrincipalTypes.PlatformUser),
            new(InvoiceFlowClaimTypes.ActorUserId, platformUserId.ToString()),
            new(InvoiceFlowClaimTypes.ActorEmailAddress, platformEmail),

            // Optional audit metadata
            new(InvoiceFlowClaimTypes.ImpersonationSessionId, sessionId.ToString()),
            new(InvoiceFlowClaimTypes.ImpersonationStartedAt, dateTime.ToString("O")),
            new(InvoiceFlowClaimTypes.ImpersonationReason, reason ?? string.Empty)
        };

        return (GenerateToken(claims, dateTime, expiresAt), "Bearer", expiresAt);
    }

    private string GenerateToken(IEnumerable<Claim> claims, DateTime startingAt, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            notBefore: startingAt,
            expires: expiresAt,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
