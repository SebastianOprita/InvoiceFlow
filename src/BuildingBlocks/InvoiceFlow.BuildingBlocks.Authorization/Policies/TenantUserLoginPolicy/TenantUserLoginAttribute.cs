using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantUserLoginAttribute : AuthorizeAttribute
{
    public TenantUserLoginAttribute()
    {
        Policy = TenantUserLoginPolicy.TenantUserLogin;
    }
}
