using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class PlatformUserAuthorizationOptionsExtensions
{
    public static AuthorizationOptions AddPlatformUserLoginPolicy(
        this AuthorizationOptions options)
    {
        options.AddPolicy(
            PlatformAuthorizationPolicy.PlatformUserLogin,
            PlatformAuthorizationPolicy.Build());

        return options;
    }
}
