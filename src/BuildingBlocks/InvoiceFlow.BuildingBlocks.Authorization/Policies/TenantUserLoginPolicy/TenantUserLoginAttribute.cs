using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class TenantUserLoginAttribute : AuthorizeAttribute
{
    public TenantUserLoginAttribute()
    {
        Policy = TenantUserLoginPolicy.TenantUserLogin;
    }
}
