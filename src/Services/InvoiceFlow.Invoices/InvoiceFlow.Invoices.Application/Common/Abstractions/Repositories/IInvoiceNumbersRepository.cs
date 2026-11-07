using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceNumbersRepository
{
    Task<string> AllocateNextWithLockAsync(
        Guid tenantId,
        InvoiceYear year,
        CancellationToken cancellationToken = default);
}
