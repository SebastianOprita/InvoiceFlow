using Microsoft.AspNetCore.Authorization;
namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PlatformUserLoginAttribute : AuthorizeAttribute
{
    public PlatformUserLoginAttribute()
    {
        Policy = PlatformUserLoginPolicy.PlatformUserLogin;
    }
}
