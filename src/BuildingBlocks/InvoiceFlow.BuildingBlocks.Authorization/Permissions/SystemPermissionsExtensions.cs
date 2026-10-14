namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class SystemPermissionsExtensions
{
    public static readonly SystemPermissions AllValidPermissions = Enum
        .GetValues<SystemPermissions>()
        .Aggregate(SystemPermissions.None, (acc, v) => acc | v);

    public static bool IsValid(this SystemPermissions systemPermission)
    {
        return (systemPermission & ~AllValidPermissions) == 0;
    }
}
