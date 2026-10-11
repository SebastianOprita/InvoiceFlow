using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

public static class TestJwtTokenFactory
{
    public static string CreateAccessToken(Guid tenantId, SystemPermission permissions, JwtSettings jwtSettings)
    {
        var dateTime = DateTime.UtcNow;
        var expiresAt = dateTime.AddMinutes(jwtSettings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, "integration-test-user@email.com"),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, ((long)permissions).ToString()),
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
            notBefore: dateTime,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreatePlatformUserAccessToken(Guid userId, JwtSettings jwtSettings)
    {
        var dateTime = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new("principal_type", "platform_user"),
            new("auth_mode", "login"),
            new("user_id", userId.ToString()),
            new("email_address", "platform-test-user@email.com"),
        };

        return GenerateToken(claims, dateTime, jwtSettings);
    }

    private static string GenerateToken(IEnumerable<Claim> claims, DateTime dateTime, JwtSettings jwtSettings)
    {
        var expiresAt = dateTime.AddMinutes(jwtSettings.AccessTokenMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: dateTime,
            expires: expiresAt,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
