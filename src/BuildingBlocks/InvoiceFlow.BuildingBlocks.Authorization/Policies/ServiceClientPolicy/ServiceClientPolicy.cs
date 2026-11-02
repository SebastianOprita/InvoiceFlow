using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class ServiceClientPolicy
{
    public const string PolicyName = "ServiceClient";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.ServiceClient)
            .RequireClaim(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.ClientCredentials)
            .Build();
    }
}
