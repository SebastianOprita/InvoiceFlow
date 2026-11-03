using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceNumberRepository
{
    Task<string> AllocateNextWithLockAsync(
        Guid tenantId,
        InvoiceYear year,
        CancellationToken cancellationToken = default);
}
