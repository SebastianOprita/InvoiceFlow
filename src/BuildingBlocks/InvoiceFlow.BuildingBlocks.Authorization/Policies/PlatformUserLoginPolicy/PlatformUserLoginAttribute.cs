using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PlatformUserLoginAttribute : AuthorizeAttribute
{
    public PlatformUserLoginAttribute()
    {
        Policy = PlatformUserLoginPolicy.PlatformUserLogin;
    }
}
