using System.Threading.RateLimiting;

namespace InvoiceFlow.Gateway;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("per-tenant", contaxt =>
            {
                var tenantId = contaxt.Request.RouteValues["tenantId"]?.ToString();

                var partitionKey = Guid.TryParse(tenantId, out var parsedTenantId)
                    ? parsedTenantId.ToString("D")
                    : "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}
