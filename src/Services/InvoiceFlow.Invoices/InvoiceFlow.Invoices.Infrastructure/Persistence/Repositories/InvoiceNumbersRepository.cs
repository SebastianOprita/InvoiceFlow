using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class InvoiceNumbersRepository(
    InvoicesDbContext context)
    : IInvoiceNumbersRepository
{
    public async Task<string> AllocateNextWithLockAsync(Guid tenantId, InvoiceYear year, CancellationToken cancellationToken = default)
    {
        var sequence = await context.InvoiceNumberSequences
            .FromSqlInterpolated($"""
                SELECT *
                FROM dbo.InvoiceNumberSequences
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE TenantId = {tenantId}
                  AND Year = {year.Value}
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (sequence is null)
        {
            sequence =  InvoiceNumberSequence.Create(
                tenantId: tenantId,
                id: Guid.CreateVersion7(),
                year: year,
                prefix:  InvoiceNumberPrefix.Create("INV"),
                startingNumber: SequenceNumber.Create(1));

            await context.InvoiceNumberSequences.AddAsync(sequence, cancellationToken);
        }

        return sequence.AllocateNextNumber();
    }
}
