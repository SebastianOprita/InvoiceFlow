using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class Organization
{
    public Guid Id { get; private set; }
    public OrganizationName Name { get; private set; }
    public OrganizationTypes Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Organization() { } // EF Core
#pragma warning restore CS8618

    private Organization(
        Guid id,
        OrganizationName name,
        OrganizationTypes type,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = name;
        Type = type;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static Organization Create(
        Guid id,
        OrganizationName name,
        OrganizationTypes type,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.OrganizationIdRequired);

        if (name is null)
            throw new DomainException(DomainErrors.OrganizationNameRequired);

        if (!Enum.IsDefined(type))
            throw new DomainException(DomainErrors.OrganizationTypeInvalid);

        return new Organization(
            id,
            name,
            type,
            createdAtUtc);
    }

    public void Rename(
        OrganizationName name,
        DateTimeOffset updatedAtUtc)
    {
        if (name is null)
            throw new DomainException(DomainErrors.OrganizationNameRequired);

        if (Name == name)
            return;

        Name = name;
        MarkAsUpdated(updatedAtUtc);
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (IsActive)
            return;

        IsActive = true;
        MarkAsUpdated(updatedAtUtc);
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkAsUpdated(updatedAtUtc);
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
