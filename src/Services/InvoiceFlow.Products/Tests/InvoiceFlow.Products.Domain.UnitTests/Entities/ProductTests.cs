using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.Entities;

public sealed class ProductTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly DateTimeOffset CreatedAtUtc =
        new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ShouldInitializeProduct_WhenArgumentsAreValid()
    {
        var createdAt = new DateTimeOffset(
            2026, 1, 1, 14, 0, 0, TimeSpan.FromHours(2));
        var description = ProductDescription.Create("Product description");

        var product = CreateProduct(
            description: description,
            createdAtUtc: createdAt);

        product.TenantId.Should().Be(TenantId);
        product.Id.Should().Be(ProductId);
        product.Code.Value.Should().Be("PROD-001");
        product.Name.Value.Should().Be("Product name");
        product.Description.Should().Be(description);
        product.UnitOfMeasure.Should().Be(GetValidUnitOfMeasures()[0]);
        product.UnitPrice.Amount.Should().Be(12.34m);
        product.TaxRate.Value.Should().Be(20m);
        product.IsActive.Should().BeTrue();
        product.CreatedAtUtc.Should().Be(createdAt.ToUniversalTime());
        product.CreatedAtUtc.Offset.Should().Be(TimeSpan.Zero);
        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldAllowNullDescription()
    {
        var product = CreateProduct(description: null);

        product.Description.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenTenantIdIsEmpty()
    {
        var act = () => CreateProduct(tenantId: Guid.Empty);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenIdIsEmpty()
    {
        var act = () => CreateProduct(id: Guid.Empty);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenCreatedAtUtcIsDefault()
    {
        var act = () => CreateProduct(
            createdAtUtc: default(DateTimeOffset));

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CreatedAtUtcRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenUnitOfMeasureIsInvalid()
    {
        var act = () => CreateProduct(
            unitOfMeasure: (UnitOfMeasure)int.MaxValue);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UnitOfMeasureInvalid);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameDescriptionAndTimestamp()
    {
        var product = CreateProduct();
        var updatedAt = CreatedAtUtc.AddHours(1);
        var description = ProductDescription.Create("Updated description");

        product.UpdateDetails(
            ProductName.Create("Updated name"),
            description,
            updatedAt);

        product.Name.Value.Should().Be("Updated name");
        product.Description.Should().Be(description);
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeCode_ShouldUpdateCodeAndTimestamp_WhenCodeIsDifferent()
    {
        var product = CreateProduct();
        var updatedAt = CreatedAtUtc.AddHours(1);

        product.ChangeCode(ProductCode.Create("PROD-002"), updatedAt);

        product.Code.Value.Should().Be("PROD-002");
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeCode_ShouldNotUpdateTimestamp_WhenCodeIsUnchanged()
    {
        var product = CreateProduct();

        product.ChangeCode(ProductCode.Create("PROD-001"), default);

        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeUnitOfMeasure_ShouldUpdateValueAndTimestamp_WhenValueIsDifferent()
    {
        var unitOfMeasures = GetValidUnitOfMeasures();
        unitOfMeasures.Should().HaveCountGreaterThan(1);

        var product = CreateProduct(unitOfMeasure: unitOfMeasures[0]);
        var updatedAt = CreatedAtUtc.AddHours(1);

        product.ChangeUnitOfMeasure(unitOfMeasures[1], updatedAt);

        product.UnitOfMeasure.Should().Be(unitOfMeasures[1]);
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeUnitOfMeasure_ShouldNotUpdateTimestamp_WhenValueIsUnchanged()
    {
        var unitOfMeasure = GetValidUnitOfMeasures()[0];
        var product = CreateProduct(unitOfMeasure: unitOfMeasure);

        product.ChangeUnitOfMeasure(unitOfMeasure, default);

        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeUnitOfMeasure_ShouldThrowDomainException_WhenValueIsInvalid()
    {
        var product = CreateProduct();

        var act = () => product.ChangeUnitOfMeasure(
            (UnitOfMeasure)int.MaxValue,
            CreatedAtUtc.AddHours(1));

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UnitOfMeasureInvalid);
    }

    [Fact]
    public void ChangeUnitPrice_ShouldUpdatePriceAndTimestamp_WhenPriceIsDifferent()
    {
        var product = CreateProduct();
        var updatedAt = CreatedAtUtc.AddHours(1);
        var newPrice = Money.Create(15.50m, CurrencyCode.Create("USD"));

        product.ChangeUnitPrice(newPrice, updatedAt);

        product.UnitPrice.Should().Be(newPrice);
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeUnitPrice_ShouldNotUpdateTimestamp_WhenPriceIsUnchanged()
    {
        var product = CreateProduct();
        var currentPrice = Money.Create(12.34m, CurrencyCode.Create("USD"));

        product.ChangeUnitPrice(currentPrice, default);

        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ChangeTaxRate_ShouldUpdateTaxRateAndTimestamp_WhenRateIsDifferent()
    {
        var product = CreateProduct();
        var updatedAt = CreatedAtUtc.AddHours(1);
        var newTaxRate = Percentage.Create(19m);

        product.ChangeTaxRate(newTaxRate, updatedAt);

        product.TaxRate.Should().Be(newTaxRate);
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void ChangeTaxRate_ShouldNotUpdateTimestamp_WhenRateIsUnchanged()
    {
        var product = CreateProduct();

        product.ChangeTaxRate(Percentage.Create(20m), default);

        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateSalesInformation_ShouldUpdateValuesAndTimestamp()
    {
        var unitOfMeasures = GetValidUnitOfMeasures();
        unitOfMeasures.Should().HaveCountGreaterThan(1);

        var product = CreateProduct(unitOfMeasure: unitOfMeasures[0]);
        var updatedAt = CreatedAtUtc.AddHours(1);
        var newPrice = Money.Create(15.50m, CurrencyCode.Create("USD"));
        var newTaxRate = Percentage.Create(19m);

        product.UpdateSalesInformation(
            unitOfMeasures[1],
            newPrice,
            newTaxRate,
            updatedAt);

        product.UnitOfMeasure.Should().Be(unitOfMeasures[1]);
        product.UnitPrice.Should().Be(newPrice);
        product.TaxRate.Should().Be(newTaxRate);
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdateSalesInformation_ShouldThrowDomainException_WhenUnitOfMeasureIsInvalid()
    {
        var product = CreateProduct();

        var act = () => product.UpdateSalesInformation(
            (UnitOfMeasure)int.MaxValue,
            Money.Create(15m, CurrencyCode.Create("USD")),
            Percentage.Create(19m),
            CreatedAtUtc.AddHours(1));

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UnitOfMeasureInvalid);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveAndTimestamp_WhenProductIsInactive()
    {
        var product = CreateProduct();
        product.Deactivate(CreatedAtUtc.AddHours(1));
        var updatedAt = CreatedAtUtc.AddHours(2);

        product.Activate(updatedAt);

        product.IsActive.Should().BeTrue();
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void Activate_ShouldNotUpdateTimestamp_WhenProductIsAlreadyActive()
    {
        var product = CreateProduct();

        product.Activate(default);

        product.IsActive.Should().BeTrue();
        product.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveAndTimestamp_WhenProductIsActive()
    {
        var product = CreateProduct();
        var updatedAt = CreatedAtUtc.AddHours(1);

        product.Deactivate(updatedAt);

        product.IsActive.Should().BeFalse();
        product.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public void Deactivate_ShouldNotUpdateTimestamp_WhenProductIsAlreadyInactive()
    {
        var product = CreateProduct();
        product.Deactivate(CreatedAtUtc.AddHours(1));
        var previousUpdatedAt = product.UpdatedAtUtc;

        product.Deactivate(default);

        product.IsActive.Should().BeFalse();
        product.UpdatedAtUtc.Should().Be(previousUpdatedAt);
    }

    private static Product CreateProduct(
        Guid? tenantId = null,
        Guid? id = null,
        ProductDescription? description = null,
        UnitOfMeasure? unitOfMeasure = null,
        DateTimeOffset? createdAtUtc = null)
    {
        return Product.Create(
            tenantId ?? TenantId,
            id ?? ProductId,
            ProductCode.Create("PROD-001"),
            ProductName.Create("Product name"),
            description,
            unitOfMeasure ?? GetValidUnitOfMeasures()[0],
            Money.Create(12.34m, CurrencyCode.Create("USD")),
            Percentage.Create(20m),
            createdAtUtc ?? CreatedAtUtc);
    }

    private static UnitOfMeasure[] GetValidUnitOfMeasures() =>
        Enum.GetValues<UnitOfMeasure>();
}
