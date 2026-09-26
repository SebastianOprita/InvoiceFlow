using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(SystemPermission permission)
    {
        Policy = PermissionPolicy.CreateName(permission);

    }
}
