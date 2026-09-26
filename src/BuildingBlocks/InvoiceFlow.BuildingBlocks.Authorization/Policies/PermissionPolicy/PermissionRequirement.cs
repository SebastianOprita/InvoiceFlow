using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(SystemPermission permission)
    {
        if (permission == SystemPermission.None || !permission.IsValid())
        {
            throw new ArgumentOutOfRangeException(nameof(permission));
        }

        Permission = permission;
    }

    public SystemPermission Permission { get; }
}
