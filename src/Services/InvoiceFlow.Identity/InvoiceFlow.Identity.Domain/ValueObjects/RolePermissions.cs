using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record RolePermissions
{
    private const long ValidPermissionsMask = (1L << 31) - 1;
    public static readonly RolePermissions None = new(0);

    public long Value { get; }

    private RolePermissions(long value)
    {
        if (!IsValid(value))
            throw new DomainException(DomainErrors.PermissionInvalid);

        Value = value;
    }

    public static RolePermissions Create(long value) => new(value);

    public RolePermissions Grant(long permission)
    {
        EnsurePermissionForChange(permission);

        return new RolePermissions(Value | permission);
    }

    public RolePermissions Revoke(long permission)
    {
        EnsurePermissionForChange(permission);

        return new RolePermissions(Value & ~permission);
    }

    public bool Has(long permission)
    {
        if (!IsValid(permission))
            throw new DomainException(DomainErrors.PermissionInvalid);

        return permission != None.Value &&
               (Value & permission) == permission;
    }

    private static void EnsurePermissionForChange(long permission)
    {
        if (permission == None.Value)
            throw new DomainException(DomainErrors.PermissionRequired);

        if (!IsValid(permission) || (permission & (permission - 1)) != 0)
            throw new DomainException(DomainErrors.PermissionInvalid);
    }

    public static bool IsValid(long value)
        => value >= 0 && (value & ~ValidPermissionsMask) == 0;

    public  static bool IsSinglePermission(long permission)
        => permission > 0 &&
           IsValid(permission) &&
           (permission & (permission - 1)) == 0;

    public override string ToString() => Value.ToString();
}
