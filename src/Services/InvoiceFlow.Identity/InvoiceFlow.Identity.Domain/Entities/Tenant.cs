using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public TenantName Name { get; private set; }
    public TenantSlug Slug { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Tenant() { } // EF Core
#pragma warning restore CS8618

    private Tenant(
        Guid id,
        TenantName name,
        TenantSlug slug,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Slug = slug;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static Tenant Create(
        Guid id,
        TenantName name,
        TenantSlug slug,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new Tenant(
            id,
            name,
            slug,
            createdAtUtc);
    }

    public void UpdateName(TenantName name, DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (Name == name)
            return;

        Name = name;
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
