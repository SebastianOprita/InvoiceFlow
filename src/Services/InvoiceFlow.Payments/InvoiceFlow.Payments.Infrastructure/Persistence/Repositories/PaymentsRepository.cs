using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Payments.Infrastructure;

internal sealed class PaymentsRepository(
    PaymentsDbContext context)
    : IPaymentsRepository
{
    public async Task<Payment?> GetPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await context.Payments.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == paymentId,
            cancellationToken);
    }

    public async Task<Payment?> GetPaymentByIdWithAllocationsAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await context.Payments.AsNoTracking()
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == paymentId,
            cancellationToken);
    }

    public async Task<Payment?> GetTrackedPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == paymentId,
            cancellationToken);
    }

    public async Task<List<Payment>> GetAllPaymentsWithAllocationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.Payments.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Include(i => i.Allocations)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetPaymentWithLockAsync(
        Guid tenantId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
        .FromSql($"""
            SELECT *
            FROM Payments
                WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = {tenantId}
              AND Id = {paymentId}
            """)
        .Include(x => x.Allocations)
        .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<InvoicePaymentBalance?> GetInvoicePaymentBalanceWithLockAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return await context.InvoicePaymentBalances
        .FromSql($"""
            SELECT *
            FROM InvoicePaymentBalances WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = {tenantId}
                AND InvoiceId = {invoiceId}
            """)
        .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<InvoicePaymentBalance?> GetInvoicePaymentBalanceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return context.InvoicePaymentBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.InvoiceId == invoiceId,
                cancellationToken);
    }

    public void AddPayment(Payment payment)
    {
        context.Payments.Add(payment);
    }

    public void AddInvoicePaymentBalance(InvoicePaymentBalance balance)
    {
        context.InvoicePaymentBalances.Add(balance);
    }
}
