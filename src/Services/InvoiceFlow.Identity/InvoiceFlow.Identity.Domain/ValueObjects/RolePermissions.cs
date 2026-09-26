using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record RolePermissions
{
    public static readonly RolePermissions None = new(SystemPermission.None);

    public SystemPermission Value { get; }

    private RolePermissions(SystemPermission value)
    {
        if (!value.IsValid())
            throw new DomainException(DomainErrors.PermissionsInvalid);

        Value = value;
    }

    public static RolePermissions Create(SystemPermission value) => new(value);

    public RolePermissions Grant(SystemPermission permission)
    {
        EnsurePermissionForChange(permission);

        return new RolePermissions(Value | permission);
    }

    public RolePermissions Revoke(SystemPermission permission)
    {
        EnsurePermissionForChange(permission);

        return new RolePermissions(Value & ~permission);
    }

    public bool Has(SystemPermission permission)
    {
        if (!permission.IsValid())
            throw new DomainException(DomainErrors.PermissionsInvalid);

        return permission != SystemPermission.None &&
               (Value & permission) == permission;
    }

    private static void EnsurePermissionForChange(SystemPermission permission)
    {
        if (permission == SystemPermission.None)
            throw new DomainException(DomainErrors.PermissionRequired);

        if (!permission.IsValid())
            throw new DomainException(DomainErrors.PermissionsInvalid);
    }

    public override string ToString() => Value.ToString();
}
