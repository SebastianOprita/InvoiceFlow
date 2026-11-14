using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IInvoicePaymentBalanceRepository
{
    Task<InvoicePaymentBalance?> GetAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    void Add(InvoicePaymentBalance balance);
}
