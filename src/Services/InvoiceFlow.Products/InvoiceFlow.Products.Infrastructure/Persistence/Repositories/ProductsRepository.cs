using InvoiceFlow.Products.Application;
using InvoiceFlow.Products.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Products.Infrastructure;

internal class ProductsRepository(ProductsDbContext dbContext) : IProductsRepository
{
    public async Task<List<Product>> GetAllProductsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == customerId,
            cancellationToken);
    }
}
