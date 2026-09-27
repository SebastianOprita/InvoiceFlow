using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class PlatformUserLoginPolicy
{
    public const string PlatformUserLogin = "PlatformUserLogin";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.PlatformUser)
            .RequireClaim(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login)
            .Build();
    }
}
