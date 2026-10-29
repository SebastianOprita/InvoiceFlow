namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceNumberRepository
{
    Task<string> AllocateNextWithLockAsync(
        Guid tenantId,
        int year,
        CancellationToken cancellationToken = default);
}
