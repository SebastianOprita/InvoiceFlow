using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IInvoicesRepository
{
    Task<Invoice?> GetInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceByIdWithLinesAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetTrackedInvoiceByIdAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceLine?> GetInvoiceLinesByIdAsync(Guid tenantId, Guid invoiceId, Guid invoiceLineId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetAllInvoicesWithLinesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<InvoicePaymentSummary?> GetPaymentSummaryAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoicePaymentSummary?> GetPaymentSummaryWithLockAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<List<InvoicePaymentSummary>> GetPaymentSummariesAsync(Guid tenantId, IReadOnlyCollection<Guid> invoiceIds, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceWithLockAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    void AddInvoice(Invoice invoice);
    void AddPaymentSummary(InvoicePaymentSummary summary);
}
