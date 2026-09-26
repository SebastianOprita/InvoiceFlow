using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class PlatformUserAttribute : AuthorizeAttribute
{
    public PlatformUserAttribute()
    {
        Policy = PlatformAuthorizationPolicy.PlatformUserLogin;
    }
}
