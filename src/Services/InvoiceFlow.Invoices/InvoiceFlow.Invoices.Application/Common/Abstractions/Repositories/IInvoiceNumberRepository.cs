namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceNumberRepository
{
    Task<string> AllocateNextAsync(
        Guid tenantId,
        int year,
        CancellationToken cancellationToken = default);
}
