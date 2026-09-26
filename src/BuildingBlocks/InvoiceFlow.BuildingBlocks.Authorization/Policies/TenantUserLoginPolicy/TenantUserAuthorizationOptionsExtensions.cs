using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class AuthorizationOptionsExtensions
{
    public static AuthorizationOptions AddTenantUserLoginPolicy(
        this AuthorizationOptions options)
    {
        options.AddPolicy(
            TenantUserLoginPolicy.TenantUserLogin,
            TenantUserLoginPolicy.Build());

        return options;
    }
}
