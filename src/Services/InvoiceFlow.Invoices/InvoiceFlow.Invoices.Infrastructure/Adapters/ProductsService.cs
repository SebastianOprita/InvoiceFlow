using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure;

internal sealed class ProductsService : IProductsService
{
    private static readonly Dictionary<string, ProductData> Products =
        new Dictionary<string, ProductData>(StringComparer.Ordinal)
        {
            ["product001"] = new(
                Guid.Parse("cda14eb7-598c-4c66-ad4d-d0db7f11bad7"),
                true,
                "product name",
                "product description",
                "RON"),
            ["product002"] = new(
                Guid.Parse("02eaed44-3446-4058-b9c8-5090feee650b"),
                false,
                "product two",
                "product description",
                "RON"),
            ["product003"] = new(
                Guid.Parse("fd8f1716-1129-493d-a64b-d991b591bf91"),
                true,
                "product three",
                "product description",
                "USD")
        };

    public Task<Result<InvoiceProductSnapshot>> GetProductAsync(
        Guid tenantId,
        string productCode,
        CancellationToken cancellationToken)
    {
        if (!Products.TryGetValue(productCode, out var data))
        {
            return Task.FromResult(
                Result<InvoiceProductSnapshot>.Failure(ApplicationErrors.ProductNotFound));
        }

        var product = InvoiceProductSnapshot.Create(
            productTenantId: tenantId,
            productId: data.Id,
            isActive: data.IsActive,
            productCode: ProductCode.Create(productCode),
            productName: ProductName.Create(data.Name),
            productDescription: ProductDescription.Create(data.Description),
            unitOfMeasure: UnitOfMeasure.Piece,
            unitPrice: Money.Create(12m, CurrencyCode.Create(data.CurrencyCode)),
            taxRate: Percentage.Create(9m));

        return Task.FromResult(Result<InvoiceProductSnapshot>.Success(product));
    }

    private sealed record ProductData(
        Guid Id,
        bool IsActive,
        string Name,
        string Description,
        string CurrencyCode);
}
