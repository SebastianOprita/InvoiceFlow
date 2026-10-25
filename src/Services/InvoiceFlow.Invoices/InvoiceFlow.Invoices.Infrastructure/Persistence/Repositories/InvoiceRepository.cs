using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class InvoiceRepository(
    InvoicesDbContext context)
    : IInvoiceRepository
{
    public async Task<Invoice?> GetInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == invoiceId,
            cancellationToken);
    }
    public async Task<Invoice?> GetForIssuingAsync(
        Guid tenantId,
        Guid Id,
        CancellationToken cancellationToken = default)
    {
        return await context.Invoices
        .FromSqlInterpolated($"""
            SELECT *
            FROM Invoices WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = {tenantId}
              AND Id = {Id}
            """)
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(cancellationToken);
    }

    public void AddInvoice(Invoice invoice)
    {
        context.Invoices.Add(invoice);
    }
}
