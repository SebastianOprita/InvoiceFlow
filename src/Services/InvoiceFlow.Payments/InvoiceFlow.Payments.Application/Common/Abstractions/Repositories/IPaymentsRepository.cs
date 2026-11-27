using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IPaymentsRepository
{
    Task<Payment?> GetPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentByIdWithAllocationsAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetTrackedPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetAllPaymentsWithAllocationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentWithLockAsync(
        Guid tenantId,
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<InvoicePaymentBalance?> GetInvoicePaymentBalanceWithLockAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);
    Task<InvoicePaymentBalance?> GetInvoicePaymentBalanceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    void AddPayment(Payment payment);
    void AddInvoicePaymentBalance(InvoicePaymentBalance balance);
}
