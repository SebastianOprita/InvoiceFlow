using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceFlow.Identity.Infrastructure;

internal class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    private const string BearerTokenType = "Bearer";
    public (string Token, DateTimeOffset ExpiresAt) GenerateRefreshToken(DateTimeOffset dateTime)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var refreshToken =  WebEncoders.Base64UrlEncode(bytes);
        var expiresAt = dateTime.AddDays(jwtSettings.Value.RefreshTokenDays);
        return (refreshToken, expiresAt);
    }

    public string CalculateTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GenerateUserAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        long permissions,
        DateTimeOffset dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, userId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, email),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, (permissions).ToString())
        };

        return (GenerateToken(claims, dateTime, expiresAt), BearerTokenType, expiresAt);
    }

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GeneratePlatformUserAccessToken(
        Guid platformUserId,
        string platformEmail,
        DateTimeOffset dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, platformUserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.PlatformUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, platformUserId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, platformEmail),
        };

        return (GenerateToken(claims, dateTime, expiresAt), BearerTokenType, expiresAt);
    }

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GenerateImpersonationToken(
        Guid platformUserId,
        string platformEmail,
        Guid targetUserId,
        Guid targetTenantId,
        string targetEmail,
        long targetPermissions,
        Guid sessionId,
        string? reason,
        DateTimeOffset dateTime)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.Value.ImpersonationTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, targetUserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Impersonation),
            new(InvoiceFlowClaimTypes.UserId, targetUserId.ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, targetEmail),
            new(InvoiceFlowClaimTypes.TenantId, targetTenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, (targetPermissions).ToString()),

            // The actor performing the impersonation
            new(InvoiceFlowClaimTypes.ActorPrincipalType, PrincipalTypes.PlatformUser),
            new(InvoiceFlowClaimTypes.ActorUserId, platformUserId.ToString()),
            new(InvoiceFlowClaimTypes.ActorEmailAddress, platformEmail),

            // Optional audit metadata
            new(InvoiceFlowClaimTypes.ImpersonationSessionId, sessionId.ToString()),
            new(InvoiceFlowClaimTypes.ImpersonationStartedAt, dateTime.ToString("O")),
            new(InvoiceFlowClaimTypes.ImpersonationReason, reason ?? string.Empty)
        };

        return (GenerateToken(claims, dateTime, expiresAt), BearerTokenType, expiresAt);
    }

    public (string Token, string TokenType, DateTimeOffset ExpiresAt) GenerateServiceClientToken(
        ServiceClient client,
        Guid tenantId,
        DateTimeOffset dateTime)
    {
        var expiresAt = dateTime.AddMinutes(
        jwtSettings.Value.ServiceClientTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.ServiceClient),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.ClientCredentials),
            new(InvoiceFlowClaimTypes.ClientId, client.ClientId.Value),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, client.Permissions.Value.ToString())
        };

        return (GenerateToken(claims, dateTime, expiresAt), BearerTokenType, expiresAt);
    }

    private string GenerateToken(IEnumerable<Claim> claims, DateTimeOffset startingAt, DateTimeOffset expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            notBefore: startingAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
