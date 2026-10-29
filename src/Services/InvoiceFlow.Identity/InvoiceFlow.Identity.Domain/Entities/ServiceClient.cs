using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class ServiceClient
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public ServiceClientId ClientId { get; private set; }
    public ServiceClientName Name { get; private set; }
    public bool IsActive { get; private set; }
    public bool CanAccessAllTenants { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private ServiceClient() { } // EF Core
#pragma warning restore CS8618

    private ServiceClient(
        Guid id,
        Guid organizationId,
        ServiceClientId clientId,
        ServiceClientName name,
        bool canAccessAllTenants,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        OrganizationId = organizationId;
        ClientId = clientId;
        Name = name;
        CanAccessAllTenants = canAccessAllTenants;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static ServiceClient Create(
        Guid id,
        Organization organization,
        ServiceClientId clientId,
        ServiceClientName name,
        DateTimeOffset createdAtUtc)
    {
        if (organization is null || organization.Id == Guid.Empty)
            throw new DomainException(DomainErrors.OrganizationIdRequired);

        if (!organization.IsActive)
            throw new DomainException(DomainErrors.OrganizationInactive);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        if (clientId is null)
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        if (name is null)
            throw new DomainException(DomainErrors.ServiceClientNameRequired);

        return new ServiceClient(
            id,
            organization.Id,
            clientId,
            name,
            organization.Type == OrganizationTypes.Internal,
            createdAtUtc);
    }

    public void Rename(
        ServiceClientName name,
        DateTimeOffset updatedAtUtc)
    {
        if (name is null)
            throw new DomainException(DomainErrors.ServiceClientNameInvalid);

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
