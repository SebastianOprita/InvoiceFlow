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
        var product1 = CreateProduct(tenantId, Guid.Parse("08faf26d-61c6-4595-a30f-48632f21f0da"), "product001", "USD", 10, dateTimeProvider.GetUtcNow());
        var product2 = CreateProduct(tenantId, Guid.Parse("a32d76f6-75ae-41ab-858b-a5255e4aa7ed"), "product002", "RON", 15, dateTimeProvider.GetUtcNow());
        var product3 = CreateProduct(tenantId, Guid.Parse("34a69fa2-c741-48d3-8184-89578b766104"), "product003", "EUR", 0, dateTimeProvider.GetUtcNow());
        await db.Products.AddRangeAsync(product1, product2, product3);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Product CreateProduct(Guid tenantId, Guid id, string productCode, string currencyCode, decimal taxPercentage, DateTimeOffset dateTime)
    {
        return Product.Create(
            tenantId,
            id,
            ProductCode.Create(productCode),
            ProductName.Create($"{productCode}-name"),
            ProductDescription.Create($"{productCode}-description"),
            UnitOfMeasure.Piece,
            Money.Create(12, CurrencyCode.Create(currencyCode)),
            Percentage.Create(taxPercentage),
            dateTime);
    }
}
