using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class TenantUserOrServiceClientOptionsExtensions
{
    public static AuthorizationOptions AddTenantUserOrServiceClientLoginPolicy(
        this AuthorizationOptions options)
    {
        options.AddPolicy(
            TenantUserOrServiceClientPolicy.PolicyName,
            TenantUserOrServiceClientPolicy.Build());

        return options;
    }
}
