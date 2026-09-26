using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class TenantUserLoginPolicy
{
    public const string TenantUserLogin = "TenantUserLogin";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser)
            .RequireClaim(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login)
            .Build();
    }
}
