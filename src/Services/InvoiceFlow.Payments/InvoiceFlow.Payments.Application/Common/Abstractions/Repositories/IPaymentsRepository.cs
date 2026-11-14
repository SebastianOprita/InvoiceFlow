using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IPaymentsRepository
{
    Task<Payment?> GetPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentByIdWithAllocationsAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetTrackedPaymentByIdAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentAllocation?> GetPaymentAllocationsByIdAsync(Guid tenantId, Guid paymentId, Guid invoiceLineId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetAllPaymentsWithAllocationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentWithLockAsync(
        Guid tenantId,
        Guid paymentId,
        CancellationToken cancellationToken = default);

    void AddPayment(Payment payment);
}
