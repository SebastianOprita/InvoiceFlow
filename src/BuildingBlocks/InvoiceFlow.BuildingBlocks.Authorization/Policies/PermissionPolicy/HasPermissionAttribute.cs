using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(SystemPermission permission)
    {
        Policy = PermissionPolicy.CreateName(permission);

    }
}
