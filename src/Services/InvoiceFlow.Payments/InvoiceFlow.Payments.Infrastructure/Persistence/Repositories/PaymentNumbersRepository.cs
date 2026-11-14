using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Payments.Infrastructure;

internal sealed class PaymentNumbersRepository(
    PaymentsDbContext context)
    : IPaymentNumbersRepository
{
    public async Task<string> AllocateNextWithLockAsync(Guid tenantId, FiscalYear year, CancellationToken cancellationToken = default)
    {
        var sequence = await context.PaymentNumberSequences
            .FromSqlInterpolated($"""
                SELECT *
                FROM dbo.PaymentNumberSequences
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE TenantId = {tenantId}
                  AND Year = {year.Value}
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (sequence is null)
        {
            sequence =  PaymentNumberSequence.Create(
                tenantId: tenantId,
                id: Guid.CreateVersion7(),
                year: year,
                prefix:  PaymentNumberPrefix.Create("PAY"),
                startingNumber: SequenceNumber.Create(1));

            await context.PaymentNumberSequences.AddAsync(sequence, cancellationToken);
        }

        return sequence.AllocateNextNumber();
    }
}
