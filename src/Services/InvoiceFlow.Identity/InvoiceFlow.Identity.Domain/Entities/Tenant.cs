using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public TenantName Name { get; private set; }
    public TenantSlug Slug { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Tenant() { } // EF Core
#pragma warning restore CS8618

    private Tenant(
        Guid id,
        TenantName name,
        TenantSlug slug,
        DateTimeOffset createdAtUtc)
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
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        return new Tenant(
            id,
            name,
            slug,
            createdAtUtc.ToUniversalTime());
    }

    public void UpdateName(TenantName name, DateTimeOffset updatedAtUtc)
    {
        if (Name == name)
            return;

        Name = name;
        MarkAsUpdated(updatedAtUtc);
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkAsUpdated(updatedAtUtc);
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (IsActive)
            return;

        IsActive = true;
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
