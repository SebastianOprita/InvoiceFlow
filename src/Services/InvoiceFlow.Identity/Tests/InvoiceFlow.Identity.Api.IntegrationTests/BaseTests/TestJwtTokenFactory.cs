using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.Identity.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

public static class TestJwtTokenFactory
{
    public static string CreateAccessToken(Guid tenantId, SystemPermissions permissions, JwtSettings jwtSettings)
    {
        var dateTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var expiresAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, "integration-test-user@email.com"),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, permissions.ToPermissions().ToString()),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: dateTime.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreatePlatformUserAccessToken(Guid userId, JwtSettings jwtSettings)
    {
        var dateTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var claims = new List<Claim>
        {
            new("principal_type", "platform_user"),
            new("auth_mode", "login"),
            new("user_id", userId.ToString()),
            new("email_address", "platform-test-user@email.com"),
        };

        return GenerateToken(claims, dateTime, jwtSettings);
    }

    private static string GenerateToken(IEnumerable<Claim> claims, DateTimeOffset dateTime, JwtSettings jwtSettings)
    {
        var expiresAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: dateTime.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreateImpersonationToken(Guid tenantId, Guid userId, SystemPermissions permissions, JwtSettings jwtSettings)
    {
        var dateTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var claims = new List<Claim>
        {
            new("principal_type", "tenant_user"),
            new("auth_mode", "impersonation"),
            new("user_id", userId.ToString()),
            new("email_address", "integration-test-user@email.com"),
            new("tenant_id", tenantId.ToString()),
            new("permissions", permissions.ToPermissions().ToString()),

            // The actor performing the impersonation
            new("actor_principal_type", "platform_user"),
            new("actor_user_id", "platformUserId"),
            new("actor_email_address", "platformEmail"),

            // Optional audit metadata
            new("impersonation_session_id", Guid.CreateVersion7().ToString()),
            new("impersonation_started_at", dateTime.ToString()),
            new("impersonation_reason", string.Empty)
        };

        return GenerateToken(claims, dateTime, jwtSettings);
    }
}
