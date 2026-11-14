using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceNumbersRepository
{
    Task<string> AllocateNextWithLockAsync(
        Guid tenantId,
        FiscalYear year,
        CancellationToken cancellationToken = default);
}
