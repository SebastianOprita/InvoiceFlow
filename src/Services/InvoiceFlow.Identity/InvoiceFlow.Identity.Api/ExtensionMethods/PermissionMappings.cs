using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Api;

public static class PermissionMappings
{
    public static RolePermissions ToRolePermissions(
        this SystemPermission permission)
    {
        return RolePermissions.Create((long)permission);
    }

    public static SystemPermission ToSystemPermission(
        this RolePermissions permissions)
    {
        return (SystemPermission)permissions.Value;
    }
}
