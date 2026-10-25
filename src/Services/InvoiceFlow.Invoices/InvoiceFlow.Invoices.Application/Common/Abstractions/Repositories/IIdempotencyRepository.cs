using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetIdempotencyRecordAsync(
        Guid tenantId,
        IdempotencyOperation operation,
        IdempotencyKey key,
        CancellationToken cancellationToken);

    void Add(IdempotencyRecord record);
}
