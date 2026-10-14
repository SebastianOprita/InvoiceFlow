using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Api;

public static class PermissionMappings
{
    public static RolePermissions ToRolePermissions(
        this SystemPermissions permission)
    {
        return RolePermissions.Create((long)permission);
    }

    public static SystemPermissions ToSystemPermission(
        this RolePermissions permissions)
    {
        return (SystemPermissions)permissions.Value;
    }
}
