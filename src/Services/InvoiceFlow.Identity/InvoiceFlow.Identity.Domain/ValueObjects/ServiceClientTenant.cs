using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class ServiceClientTenant
{
    public Guid ServiceClientId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private ServiceClientTenant() { } // EF Core
#pragma warning restore CS8618

    private ServiceClientTenant(
        Guid serviceClientId,
        Guid tenantId,
        DateTimeOffset createdAtUtc)
    {
        ServiceClientId = serviceClientId;
        TenantId = tenantId;
        CreatedAtUtc = createdAtUtc;
    }

    internal static ServiceClientTenant Create(
        Guid serviceClientId,
        Guid tenantId,
        DateTimeOffset createdAtUtc)
    {
        if (serviceClientId == Guid.Empty)
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        return new ServiceClientTenant(
            serviceClientId,
            tenantId,
            createdAtUtc);
    }
}
