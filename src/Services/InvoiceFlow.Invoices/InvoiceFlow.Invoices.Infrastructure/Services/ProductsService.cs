using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class ProductsService : IProductsService
{
    public Task<Result<InvoiceProductSnapshot>> GetProductAsync(Guid tenantId, string productCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return Task.FromResult(
                Result<InvoiceProductSnapshot>.Failure(
                    ApplicationErrors.ProductNotFound));

        var product = InvoiceProductSnapshot.Create(
            productTenantId: tenantId,
            productId: Guid.Parse("08faf26d-61c6-4595-a30f-48632f21f0da"),
            isActive: true,
            productCode: ProductCode.Create(productCode),
            productName: ProductName.Create("product name"),
            productDescription: ProductDescription.Create("product description"),
            unitOfMeasure: UnitOfMeasure.Piece,
            unitPrice: Money.Create(12m, CurrencyCode.Create("RON")),
            taxRate: Percentage.Create(9m));

        return Task.FromResult(
            Result<InvoiceProductSnapshot>.Success(product));
    }
}
