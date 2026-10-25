using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface ICustomerService
{
    Task<Result<InvoiceCustomerSnapshot>> GetCustomerAsync(Guid tenantId, string customerCode, CancellationToken cancellationToken);
}
