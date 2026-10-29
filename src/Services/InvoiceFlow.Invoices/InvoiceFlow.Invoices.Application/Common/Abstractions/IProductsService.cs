using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public interface IProductsService
{
    Task<Result<InvoiceProductSnapshot>> GetProductAsync(Guid tenantId, string productCode, CancellationToken cancellationToken);
}
