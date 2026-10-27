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

    public async Task<Invoice?> GetInvoiceByIdWithLineAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == invoiceId,
            cancellationToken);
    }

    public async Task<Invoice?> GetTrackedInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == invoiceId,
            cancellationToken);
    }

    public async Task<InvoiceLine?> GetInvoiceLineByIdAsync(Guid tenantId, Guid invoiceId, Guid invoiceLineId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Id == invoiceId)
            .SelectMany(i => i.Lines)
            .FirstOrDefaultAsync(
                line => line.Id == invoiceLineId,
                cancellationToken);
    }

    public async Task<List<Invoice>> GetAllInvoicesWithLinesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Include(i => i.Lines)
            .ToListAsync(cancellationToken);
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
