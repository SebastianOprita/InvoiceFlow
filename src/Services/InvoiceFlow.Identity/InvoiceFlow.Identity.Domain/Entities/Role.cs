using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class Role
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public RoleName Name { get; private set; }
    public RoleDescription? Description { get; private set; }
    public RolePermissions Permissions { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    // Navigation property
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

#pragma warning disable CS8618
    private Role() { } // EF Core
#pragma warning restore CS8618

    private Role(
        Guid tenantId,
        Guid id,
        RoleName name,
        DateTimeOffset createdAtUtc,
        RoleDescription? description = null,
        RolePermissions? permissions = null)
    {
        TenantId = tenantId;
        Id = id;
        Name = name;
        Description = description;
        Permissions = permissions ?? RolePermissions.None;
        CreatedAtUtc = createdAtUtc;
    }

    public static Role Create(
        Guid tenantId,
        Guid id,
        RoleName name,
        DateTimeOffset createdAtUtc,
        RoleDescription? description = null,
        RolePermissions? permissions = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        return new Role(tenantId, id, name, createdAtUtc, description, permissions);
    }

    public void UpdateDetails(RoleName name, DateTimeOffset updatedAtUtc, RoleDescription? description = null)
    {
        if (Name == name && Description == description)
            return;

        Name = name;
        Description = description;
        MarkAsUpdated(updatedAtUtc);
    }

    public void GrantPermission(long permission, DateTimeOffset updatedAtUtc)
    {
        var newPermissions = Permissions.Grant(permission);
        if (newPermissions == Permissions)
            return;

        Permissions = newPermissions;
        MarkAsUpdated(updatedAtUtc);
    }

    public void RevokePermission(long permission, DateTimeOffset updatedAtUtc)
    {
        var newPermissions = Permissions.Revoke(permission);
        if (Permissions == newPermissions)
            return;

        Permissions = newPermissions;
        MarkAsUpdated(updatedAtUtc);
    }

    public bool HasPermission(long permission)
    {
        return Permissions.Has(permission);
    }

    public void RemoveFromAllUsers()
    {
        _userRoles.Clear();
    }

    private void MarkAsUpdated(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        updatedAtUtc = updatedAtUtc.ToUniversalTime();

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        UpdatedAtUtc = updatedAtUtc;
    }
}
