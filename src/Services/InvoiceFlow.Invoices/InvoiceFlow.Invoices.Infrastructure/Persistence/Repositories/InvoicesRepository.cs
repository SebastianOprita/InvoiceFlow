using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class InvoicesRepository(
    InvoicesDbContext context)
    : IInvoicesRepository
{
    public async Task<Invoice?> GetInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await context.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == invoiceId,
            cancellationToken);
    }

    public async Task<Invoice?> GetInvoiceByIdWithLinesAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default)
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

    public async Task<InvoiceLine?> GetInvoiceLinesByIdAsync(Guid tenantId, Guid invoiceId, Guid invoiceLineId, CancellationToken cancellationToken = default)
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

    public async Task<InvoicePaymentSummary?> GetPaymentSummaryAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await context.InvoicePaymentSummaries.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.InvoiceId == invoiceId,
                cancellationToken);
    }

    public async Task<InvoicePaymentSummary?> GetPaymentSummaryWithLockAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await context.InvoicePaymentSummaries
            .FromSql($"""
                SELECT *
                FROM InvoicePaymentSummaries
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE TenantId = {tenantId}
                  AND InvoiceId = {invoiceId}
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<InvoicePaymentSummary>> GetPaymentSummariesAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> invoiceIds,
        CancellationToken cancellationToken = default)
    {
        if (invoiceIds.Count == 0)
            return [];

        return await context.InvoicePaymentSummaries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && invoiceIds.Contains(x.InvoiceId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetInvoiceWithLockAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await context.Invoices
            .FromSql($"""
                SELECT *
                FROM Invoices
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE TenantId = {tenantId}
                  AND Id = {invoiceId}
                """)
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void AddInvoice(Invoice invoice)
    {
        context.Invoices.Add(invoice);
    }

    public void AddPaymentSummary(InvoicePaymentSummary summary)
    {
        context.InvoicePaymentSummaries.Add(summary);
    }
}
