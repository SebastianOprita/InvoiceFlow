using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class ProductsService : IProductsService
{
    public Task<Result<InvoiceProductSnapshot>> GetProductAsync(Guid tenantId, string productCode, CancellationToken cancellationToken)
    {
        if (productCode == "product001")
        {
            var product = InvoiceProductSnapshot.Create(
                productTenantId: tenantId,
                productId: Guid.Parse("cda14eb7-598c-4c66-ad4d-d0db7f11bad7"),
                isActive: true,
                productCode: ProductCode.Create("product001"),
                productName: ProductName.Create("product name"),
                productDescription: ProductDescription.Create("product description"),
                unitOfMeasure: UnitOfMeasure.Piece,
                unitPrice: Money.Create(12m, CurrencyCode.Create("RON")),
                taxRate: Percentage.Create(9m));

            return Task.FromResult(
                Result<InvoiceProductSnapshot>.Success(product));
        }

        if (productCode == "product002")
        {
            var product = InvoiceProductSnapshot.Create(
                productTenantId: tenantId,
                productId: Guid.Parse("02eaed44-3446-4058-b9c8-5090feee650b"),
                isActive: false,
                productCode: ProductCode.Create("product002"),
                productName: ProductName.Create("product two"),
                productDescription: ProductDescription.Create("product description"),
                unitOfMeasure: UnitOfMeasure.Piece,
                unitPrice: Money.Create(12m, CurrencyCode.Create("RON")),
                taxRate: Percentage.Create(9m));

            return Task.FromResult(
                Result<InvoiceProductSnapshot>.Success(product));
        }

        if (productCode == "product003")
        {
            var product = InvoiceProductSnapshot.Create(
                productTenantId: tenantId,
                productId: Guid.Parse("fd8f1716-1129-493d-a64b-d991b591bf91"),
                isActive: true,
                productCode: ProductCode.Create("product003"),
                productName: ProductName.Create("product three"),
                productDescription: ProductDescription.Create("product description"),
                unitOfMeasure: UnitOfMeasure.Piece,
                unitPrice: Money.Create(12m, CurrencyCode.Create("USD")),
                taxRate: Percentage.Create(9m));

            return Task.FromResult(
                Result<InvoiceProductSnapshot>.Success(product));
        }

        return Task.FromResult(
            Result<InvoiceProductSnapshot>.Failure(
                ApplicationErrors.ProductNotFound));
    }
}
