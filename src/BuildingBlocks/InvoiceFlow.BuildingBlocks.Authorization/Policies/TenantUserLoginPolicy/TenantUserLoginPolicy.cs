using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class TenantUserLoginPolicy
{
    public const string PolicyName = "TenantUserLogin";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser)
            .RequireClaim(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login)
            .RequireAssertion(context =>
            {
                var userId = context.User.FindFirst(
                    InvoiceFlowClaimTypes.UserId)?.Value;

                var tenantId = context.User.FindFirst(
                    InvoiceFlowClaimTypes.TenantId)?.Value;

                return Guid.TryParse(userId, out var parsedUserId) &&
                       parsedUserId != Guid.Empty &&
                       Guid.TryParse(tenantId, out var parsedTenantId) &&
                       parsedTenantId != Guid.Empty;
            })
            .Build();
    }
}
