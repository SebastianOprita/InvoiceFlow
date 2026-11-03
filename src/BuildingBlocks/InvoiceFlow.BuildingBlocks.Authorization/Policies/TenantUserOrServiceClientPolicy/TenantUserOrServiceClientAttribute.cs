using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantUserOrServiceClientAttribute : AuthorizeAttribute
{
    public TenantUserOrServiceClientAttribute()
    {
        Policy = TenantUserOrServiceClientPolicy.PolicyName;
    }
}
