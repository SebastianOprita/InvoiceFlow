using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class PlatformUser
{
    public Guid Id { get; private set; }
    public UserEmail Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private PlatformUser() { } // EF Core
#pragma warning restore CS8618

    private PlatformUser(
        Guid id,
        UserEmail email,
        PasswordHash passwordHash,
        FirstName firstName,
        LastName lastName,
        DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static PlatformUser Create(
        Guid id,
        UserEmail email,
        PasswordHash passwordHash,
        FirstName firstName,
        LastName lastName,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new PlatformUser(
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
