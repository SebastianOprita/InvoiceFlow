using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface IPaymentNumbersRepository
{
    Task<string> AllocateNextWithLockAsync(
        Guid tenantId,
        FiscalYear year,
        CancellationToken cancellationToken = default);
}
