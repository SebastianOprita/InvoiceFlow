using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceByIdWithLineAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetTrackedInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceLine?> GetInvoiceLineByIdAsync(Guid tenantId, Guid invoiceId, Guid invoiceLineId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetAllInvoicesWithLinesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceWithLockAsync(
        Guid tenantId,
        Guid Id,
        CancellationToken cancellationToken = default);

    void AddInvoice(Invoice invoice);
}
