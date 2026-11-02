using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class PlatformUserLoginPolicy
{
    public const string PolicyName = "PlatformUserLogin";

    public static AuthorizationPolicy Build()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.PlatformUser)
            .RequireClaim(InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login)
            .RequireAssertion(context =>
            {
                var platformUserId = context.User.FindFirst(
                    InvoiceFlowClaimTypes.UserId)?.Value;

                return Guid.TryParse(platformUserId, out var parsedPlatformUserId) &&
                       parsedPlatformUserId != Guid.Empty;
            })
            .Build();
    }
}
