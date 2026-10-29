using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Invoices.Infrastructure;

internal class IdempotencyRepository(InvoicesDbContext dbContext) : IIdempotencyRepository
{
    public async Task<IdempotencyRecord?> GetIdempotencyRecordAsync(Guid tenantId, IdempotencyOperation operation, IdempotencyKey key, CancellationToken cancellationToken)
    {
        return await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record =>
                    record.TenantId == tenantId &&
                    record.Operation == operation &&
                    record.Key == key,
                cancellationToken);
    }

    public void Add(IdempotencyRecord record)
    {
        dbContext.IdempotencyRecords.Add(record);
    }
}
