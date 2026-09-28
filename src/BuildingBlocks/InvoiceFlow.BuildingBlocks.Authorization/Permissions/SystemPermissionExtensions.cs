namespace InvoiceFlow.BuildingBlocks.Authorization;

public static class SystemPermissionExtensions
{
    public static readonly SystemPermission AllValidPermissions = Enum
        .GetValues<SystemPermission>()
        .Aggregate(SystemPermission.None, (acc, v) => acc | v);

    public static bool IsValid(this SystemPermission systemPermission)
    {
        return (systemPermission & ~AllValidPermissions) == 0;
    }
}
