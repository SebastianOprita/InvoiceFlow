using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceRepository
{
    Task<Invoice?> GetForIssuingAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

}
