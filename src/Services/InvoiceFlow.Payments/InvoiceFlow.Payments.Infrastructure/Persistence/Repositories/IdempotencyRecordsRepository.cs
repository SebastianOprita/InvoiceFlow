using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Payments.Infrastructure;

internal class IdempotencyRecordsRepository(PaymentsDbContext dbContext) : IIdempotencyRecordsRepository
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
