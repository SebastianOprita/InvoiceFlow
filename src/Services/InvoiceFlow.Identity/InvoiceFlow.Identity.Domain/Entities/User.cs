using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class User
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public UserEmail Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // Navigation property
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

#pragma warning disable CS8618
    private User() { } // EF Core
#pragma warning restore CS8618

    private User(
        Guid tenantId,
        Guid id,
        UserEmail email,
        PasswordHash passwordHash,
        FirstName firstName,
        LastName lastName,
        DateTime createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static User Create(
        Guid tenantId,
        Guid id,
        UserEmail email,
        PasswordHash passwordHash,
        FirstName firstName,
        LastName lastName,
        DateTime createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new User(
            tenantId,
            id,
            email,
            passwordHash,
            firstName,
            lastName,
            createdAtUtc);
    }

    public void UpdateProfile(FirstName firstName, LastName lastName, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (FirstName == firstName && LastName == lastName)
            return;

        FirstName = firstName;
        LastName = lastName;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeEmail(UserEmail email, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (Email == email)
            return;

        Email = email;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangePasswordHash(PasswordHash passwordHash, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (PasswordHash == passwordHash)
            return;

        PasswordHash = passwordHash;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void AssignRole(Guid roleId, DateTime assignedAtUtc)
    {
        if (roleId == Guid.Empty)
            throw new DomainException(DomainErrors.RoleIdRequired);

        if (assignedAtUtc == default)
            throw new DomainException(DomainErrors.AssignedAtUtcRequired);

        if (assignedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.AssignedAtUtcNotUtc);

        if (_userRoles.Any(x => x.RoleId == roleId))
            return;

        _userRoles.Add(UserRole.Create(TenantId, Id, roleId, assignedAtUtc));
    }

    public void RevokeRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new DomainException(DomainErrors.RoleIdRequired);

        var userRole = _userRoles.FirstOrDefault(x => x.RoleId == roleId);
        if (userRole is null)
            return;

        _userRoles.Remove(userRole);
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
