using InvoiceFlow.Products.Domain;

namespace InvoiceFlow.Products.Application;

public interface IProductsRepository
{
    public Task<List<Product>> GetAllProductsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    public Task<Product?> GetProductByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
}
