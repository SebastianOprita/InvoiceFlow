using InvoiceFlow.Products.Domain;

namespace InvoiceFlow.Products.Application;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            TenantId: product.TenantId,
            Id: product.Id,
            ProductCode: product.Code.Value,
            ProductName: product.Name.Value,
            ProductDescription: product.Description?.Value,
            UnitOfMeasure: product.UnitOfMeasure,
            UnitPrice: product.UnitPrice.ToDto(),
            TaxRate: product.TaxRate.Value,
            IsActive: product.IsActive,
            CreatedAtUtc: product.CreatedAtUtc,
            UpdatedAtUtc: product.UpdatedAtUtc);
    }

    public static MoneyDto ToDto(this Money money)
    {
        return new MoneyDto(
            Amount: money.Amount,
            CurrencyCode: money.CurrencyCode.Value,
            DecimalPlaces: money.CurrencyCode.DecimalPlaces);
    }
}
