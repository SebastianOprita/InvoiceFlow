using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class UserRole
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

#pragma warning disable CS8618
    private UserRole() { } // EF Core
#pragma warning restore CS8618

    private UserRole(Guid tenantId, Guid userId, Guid roleId, DateTime assignedAtUtc)
    {
        TenantId = tenantId;
        UserId = userId;
        RoleId = roleId;
        AssignedAtUtc = assignedAtUtc;
    }

    public static UserRole Create(Guid tenantId, Guid userId, Guid roleId, DateTime assignedAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (userId == Guid.Empty)
            throw new DomainException(DomainErrors.UserIdRequired);

        if (roleId == Guid.Empty)
            throw new DomainException(DomainErrors.RoleIdRequired);

        if (assignedAtUtc == default)
            throw new DomainException(DomainErrors.AssignedAtUtcRequired);

        if (assignedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.AssignedAtUtcNotUtc);

        return new UserRole(tenantId,
            userId,
            roleId,
            assignedAtUtc);
    }
}
