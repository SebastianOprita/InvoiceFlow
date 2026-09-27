using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(SystemPermission permission)
    {
        Policy = PermissionPolicy.CreateName(permission);

    }
}
