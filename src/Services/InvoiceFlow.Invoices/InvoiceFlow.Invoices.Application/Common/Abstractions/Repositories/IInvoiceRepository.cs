using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetForIssuingAsync(
        Guid tenantId,
        Guid Id,
        CancellationToken cancellationToken = default);

    void AddInvoice(Invoice invoice);
}
