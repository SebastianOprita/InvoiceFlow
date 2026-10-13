using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

[AttributeUsage(AttributeTargets.Method)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(SystemPermissions permission)
    {
        Policy = PermissionPolicy.CreateName(permission);

    }
}
