using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IIdempotencyRecordsRepository
{
    Task<IdempotencyRecord?> GetIdempotencyRecordAsync(
        Guid tenantId,
        IdempotencyOperation operation,
        IdempotencyKey key,
        CancellationToken cancellationToken);

    void Add(IdempotencyRecord record);
}
