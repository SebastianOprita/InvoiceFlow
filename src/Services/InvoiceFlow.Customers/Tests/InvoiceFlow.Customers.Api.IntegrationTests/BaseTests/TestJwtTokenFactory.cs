using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

public static class TestJwtTokenFactory
{
    public static string CreateAccessToken(Guid tenantId, SystemPermission permissions)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;


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
}
