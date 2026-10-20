using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class InvoiceRepository(
    InvoicesDbContext context)
    : IInvoiceRepository
{
    public async Task<Invoice?> GetForIssuingAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await context.Invoices
        .FromSqlInterpolated($"""
            SELECT *
            FROM Invoices WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = {tenantId}
              AND Id = {invoiceId}
            """)
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(cancellationToken);
    }
}
