using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed class IdempotencyRecord
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public IdempotencyOperation Operation { get; private set; }
    public IdempotencyKey Key { get; private set; }
    public RequestHash RequestHash { get; private set; }
    public Guid ResourceId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private IdempotencyRecord() { } // EF Core
#pragma warning restore CS8618

    private IdempotencyRecord(
        Guid tenantId,
        Guid id,
        IdempotencyOperation operation,
        IdempotencyKey key,
        RequestHash requestHash,
        Guid resourceId,
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Operation = operation;
        Key = key;
        RequestHash = requestHash;
        ResourceId = resourceId;
        CreatedAtUtc = createdAtUtc;
    }

    public static IdempotencyRecord Create(
        Guid tenantId,
        IdempotencyOperation operation,
        IdempotencyKey key,
        RequestHash requestHash,
        Guid resourceId,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (!Enum.IsDefined(operation))
            throw new DomainException(DomainErrors.IdempotencyOperationInvalid);

        if (resourceId == Guid.Empty)
            throw new DomainException(DomainErrors.ResourceIdRequired);

        return new IdempotencyRecord(
            tenantId,
            Guid.CreateVersion7(),
            operation,
            key,
            requestHash,
            resourceId,
            createdAtUtc);
    }

    public bool MatchesRequest(RequestHash requestHash) =>
        RequestHash == requestHash;
}
