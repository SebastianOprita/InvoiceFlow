using InvoiceFlow.BuildingBlocks.Authorization.Permissions;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public static class PermissionPolicy
{
    public const string Prefix = "Permission:";

    public static string CreateName(SystemPermission permission)
        => $"{Prefix}{(long)permission}";
}
