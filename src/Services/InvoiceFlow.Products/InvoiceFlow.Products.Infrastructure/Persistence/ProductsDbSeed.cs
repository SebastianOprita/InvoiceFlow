using InvoiceFlow.Products.Domain;

namespace InvoiceFlow.Products.Infrastructure;

public static class ProductsDbSeed
{
    public static async Task SeedAsync(
        ProductsDbContext db,
        BuildingBlocks.Application.ISystemDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a");
        var product1 = CreateProduct(tenantId, "product1", "USD", 10, dateTimeProvider.GetUtcNow());
        var product2 = CreateProduct(tenantId, "product2", "RON", 15, dateTimeProvider.GetUtcNow());
        var product3 = CreateProduct(tenantId, "product3", "EUR", 0, dateTimeProvider.GetUtcNow());
        await db.Products.AddRangeAsync(product1, product2, product3);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Product CreateProduct(Guid tenantId, string productCode, string currencyCode, decimal taxPercentage, DateTimeOffset dateTime)
    {
        return Product.Create(
            tenantId,
            Guid.CreateVersion7(),
            ProductCode.Create(productCode),
            ProductName.Create($"{productCode}-name"),
            ProductDescription.Create($"{productCode}-description"),
            UnitOfMeasure.Piece,
            Money.Create(123, CurrencyCode.Create(currencyCode)),
            Percentage.Create(taxPercentage),
            dateTime);
    }
}
