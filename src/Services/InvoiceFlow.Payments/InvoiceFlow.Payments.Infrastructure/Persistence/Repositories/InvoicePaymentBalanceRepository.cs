using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Payments.Infrastructure;

internal sealed class InvoicePaymentBalanceRepository(
    PaymentsDbContext context)
    : IInvoicePaymentBalanceRepository
{
    public Task<InvoicePaymentBalance?> GetAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        return context.InvoicePaymentBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.InvoiceId == invoiceId,
                cancellationToken);
    }

    public void Add(InvoicePaymentBalance balance)
    {
        context.InvoicePaymentBalances.Add(balance);
    }
}
