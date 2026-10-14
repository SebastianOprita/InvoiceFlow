using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddInvoiceFlowAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("JwtSettings")
            .Get<JwtValidationSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

        if (string.IsNullOrWhiteSpace(settings.Issuer))
            throw new InvalidOperationException("JwtSettings:Issuer is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("JwtSettings:Audience is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Secret))
            throw new InvalidOperationException("JwtSettings:Secret is not configured.");

        if (Encoding.UTF8.GetByteCount(settings.Secret) < 32)
            throw new InvalidOperationException("JwtSettings:Secret must be at least 32 bytes.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = settings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),

                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
