using Microsoft.AspNetCore.Authorization;

namespace InvoiceFlow.BuildingBlocks.Authorization.Policies;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(SystemPermissions permission)
    {
        if (permission == SystemPermissions.None || !permission.IsValid())
        {
            throw new ArgumentOutOfRangeException(nameof(permission));
        }

        Permission = permission;
    }

    public SystemPermissions Permission { get; }
}
