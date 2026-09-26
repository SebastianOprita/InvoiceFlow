using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.BuildingBlocks.Authorization;

public sealed class TenantScopeFilter(ILogger<TenantScopeFilter> logger) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)

    {
        if (!context.RouteData.Values.TryGetValue("tenantId", out var tenantValue))
        {
            logger.LogWarning(
                "Tenant-scoped request rejected because tenantId is missing from route. Path: {Path}",
                context.HttpContext.Request.Path);

            context.Result = new BadRequestObjectResult(new
            {
                message = "Tenant-scoped endpoints must define a 'tenantId' route parameter."
            });

            return;
        }

        if (!Guid.TryParse(tenantValue?.ToString(), out var routeTenantId))
        {
            logger.LogWarning(
                "Tenant-scoped request rejected because tenantId is invalid. TenantId: {TenantId}, Path: {Path}",
                tenantValue,
                context.HttpContext.Request.Path);

            context.Result = new BadRequestObjectResult(new
            {
                message = "Invalid tenantId."
            });

            return;
        }

        var user = context.HttpContext.User;

        // Anonymous endpoints (e.g. login) don't have a tenant claim
        // to validate against the route tenant.
        if (user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var tenantClaim = user.FindFirst(InvoiceFlowClaimTypes.TenantId)?.Value;

        if (!Guid.TryParse(tenantClaim, out var tokenTenantId))
        {
            logger.LogWarning(
                "Tenant-scoped request rejected because authenticated user has a missing or invalid tenant claim. " +
                "RouteTenantId: {RouteTenantId}, Path: {Path}",
                routeTenantId,
                context.HttpContext.Request.Path);

            context.Result = new ForbidResult();
            return;
        }

        if (routeTenantId != tokenTenantId)
        {
            logger.LogWarning(
                "Tenant-scoped request rejected because route tenant does not match token tenant. " +
                "RouteTenantId: {RouteTenantId}, TokenTenantId: {TokenTenantId}, Path: {Path}",
                routeTenantId,
                tokenTenantId,
                context.HttpContext.Request.Path);

            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
