using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

public static class TestJwtTokenFactory
{
    public static string CreateAccessToken(Guid tenantId, SystemPermissions permissions, JwtValidationSettings jwtSettings)
    {
        var claims = new List<Claim>
        {
            new(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            new(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login),
            new(InvoiceFlowClaimTypes.UserId, Guid.CreateVersion7().ToString()),
            new(InvoiceFlowClaimTypes.EmailAddress, "integration-test-user@email.test"),
            new(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()),
            new(InvoiceFlowClaimTypes.Permissions, ((long)permissions).ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            notBefore: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcDateTime,
            expires: new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
