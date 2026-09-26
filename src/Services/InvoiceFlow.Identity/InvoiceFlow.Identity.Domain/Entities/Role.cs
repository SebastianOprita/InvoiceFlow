using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class Role
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public RoleName Name { get; private set; }
    public RoleDescription? Description { get; private set; }
    public RolePermissions Permissions { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

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
        DateTime createdAtUtc,
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
        DateTime createdAtUtc,
        RoleDescription? description = null,
        RolePermissions? permissions = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new Role(tenantId, id, name, createdAtUtc, description, permissions);
    }

    public void UpdateDetails(RoleName name, DateTime updatedAtUtc, RoleDescription? description = null)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (Name == name && Description == description)
            return;

        Name = name;
        Description = description;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void GrantPermission(SystemPermission permission, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        var newPermissions = Permissions.Grant(permission);
        if (newPermissions == Permissions)
            return;

        Permissions = newPermissions;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void RevokePermission(SystemPermission permission, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        var newPermissions = Permissions.Revoke(permission);
        if (Permissions == newPermissions)
            return;

        Permissions = newPermissions;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetPermissions(RolePermissions permissions, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (Permissions == permissions)
            return;

        Permissions = permissions;
        UpdatedAtUtc = updatedAtUtc;
    }

    public bool HasPermission(SystemPermission permission)
    {
        return Permissions.Has(permission);
    }

    public void RemoveFromAllUsers()
    {
        _userRoles.Clear();
    }

    private void EnsureValidUpdateTime(DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        if (updatedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.UpdatedAtUtcNotUtc);

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);
    }
}
