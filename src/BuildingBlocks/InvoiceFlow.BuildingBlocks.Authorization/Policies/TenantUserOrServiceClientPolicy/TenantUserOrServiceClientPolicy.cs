using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class TenantUserOrServiceClientPolicy
{
    public const string PolicyName = "TenantUserOrServiceClient";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(context =>
            {
                var principalType = context.User.FindFirst(
                    InvoiceFlowClaimTypes.PrincipalType)?.Value;

                var authMode = context.User.FindFirst(
                    InvoiceFlowClaimTypes.AuthMode)?.Value;

                return (principalType == PrincipalTypes.TenantUser &&
                        authMode == AuthenticationModes.Login)
                    ||
                       (principalType == PrincipalTypes.ServiceClient &&
                        authMode == AuthenticationModes.ClientCredentials);
            })
            .Build();
    }
}
