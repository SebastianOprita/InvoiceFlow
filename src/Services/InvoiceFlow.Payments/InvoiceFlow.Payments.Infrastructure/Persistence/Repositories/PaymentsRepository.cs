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

    public async Task<PaymentAllocation?> GetPaymentAllocationsByIdAsync(Guid tenantId, Guid paymentId, Guid invoiceLineId, CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Id == paymentId)
            .SelectMany(i => i.Allocations)
            .FirstOrDefaultAsync(
                allocation => allocation.Id == invoiceLineId,
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
        .FromSqlInterpolated($"""
            SELECT *
            FROM Invoices WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = {tenantId}
              AND Id = {paymentId}
            """)
        .Include(x => x.Allocations)
        .SingleOrDefaultAsync(cancellationToken);
    }

    public void AddPayment(Payment payment)
    {
        context.Payments.Add(payment);
    }
}
