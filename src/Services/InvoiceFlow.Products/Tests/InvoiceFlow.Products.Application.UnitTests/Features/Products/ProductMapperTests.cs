using FluentAssertions;
using InvoiceFlow.Products.Domain;
using Xunit;

namespace InvoiceFlow.Products.Application.UnitTests.Features;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToDto_ShouldMapProductProperties()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createdAtUtc = new DateTimeOffset(
            2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var updatedAtUtc = createdAtUtc.AddHours(1);
        var unitOfMeasure = Enum.GetValues<UnitOfMeasure>()[0];
        var unitPrice = Money.Create(12.34m, CurrencyCode.Create("USD"));
        var taxRate = Percentage.Create(20m);
        var description = ProductDescription.Create("Product description");

        var product = Product.Create(
            tenantId,
            productId,
            ProductCode.Create("PROD-001"),
            ProductName.Create("Product name"),
            description,
            unitOfMeasure,
            unitPrice,
            taxRate,
            createdAtUtc);

        product.ChangeCode(ProductCode.Create("PROD-002"), updatedAtUtc);

        var result = product.ToDto();

        result.TenantId.Should().Be(tenantId);
        result.Id.Should().Be(productId);
        result.ProductCode.Should().Be("PROD-002");
        result.ProductName.Should().Be("Product name");
        result.ProductDescription.Should().Be("Product description");
        result.UnitOfMeasure.Should().Be(unitOfMeasure);
        result.UnitPrice.Amount.Should().Be(12.34m);
        result.UnitPrice.CurrencyCode.Should().Be("USD");
        result.UnitPrice.DecimalPlaces.Should().Be(2);
        result.TaxRate.Should().Be(20m);
        result.IsActive.Should().BeTrue();
        result.CreatedAtUtc.Should().Be(createdAtUtc);
        result.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapNullDescription()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ProductCode.Create("PROD-001"),
            ProductName.Create("Product name"),
            null,
            Enum.GetValues<UnitOfMeasure>()[0],
            Money.Create(10m, CurrencyCode.Create("USD")),
            Percentage.Create(0m),
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

        var result = product.ToDto();

        result.ProductDescription.Should().BeNull();
    }

    [Fact]
    public void MoneyToDto_ShouldMapAmountCurrencyCodeAndDecimalPlaces()
    {
        var money = Money.Create(1.234m, CurrencyCode.Create("BHD"));

        var result = money.ToDto();

        result.Amount.Should().Be(1.234m);
        result.CurrencyCode.Should().Be("BHD");
        result.DecimalPlaces.Should().Be(3);
    }
}
