using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public interface ICustomersService
{
    Task<Result<PaymentCustomerSnapshot>> GetCustomerAsync(Guid tenantId, string customerCode, CancellationToken cancellationToken);
}
