using InvoiceFlow.BuildingBlocks.Authorization;

namespace InvoiceFlow.Identity.Api;

public static class PermissionMappings
{
    public static long ToPermissions(this SystemPermissions permission)
    {
        return (long)permission;
    }
}
