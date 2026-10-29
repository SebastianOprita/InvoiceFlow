using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Snapshots;

public sealed class InvoiceProductSnapshotTests
{
    [Fact]
    public void Create_CopiesAllProductFields()
    {
        var tenantId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        var product = CreateProduct(tenantId, productId);

        product.ProductTenantId.Should().Be(tenantId);
        product.ProductId.Should().Be(productId);
        product.IsActive.Should().BeTrue();
        product.ProductCode.Value.Should().Be("PROD-001");
        product.ProductName.Value.Should().Be("Consulting");
        product.ProductDescription!.Value.Should().Be("Professional services");
        product.UnitOfMeasure.Should().Be(UnitOfMeasure.Hour);
        product.UnitPrice.Amount.Should().Be(125.50m);
        product.UnitPrice.CurrencyCode.Value.Should().Be("RON");
        product.TaxRate.Value.Should().Be(19m);
    }

    [Fact]
    public void Create_PreservesNullOptionalDescriptionAndInactiveStatus()
    {
        var product = InvoiceProductSnapshot.Create(
            Guid.CreateVersion7(), Guid.CreateVersion7(), false,
            ProductCode.Create("PROD-002"), ProductName.Create("License"),
            productDescription: null,
            unitOfMeasure: UnitOfMeasure.License,
            unitPrice: Money.Create(10m, CurrencyCode.Create("EUR")),
            taxRate: Percentage.Zero);

        product.IsActive.Should().BeFalse();
        product.ProductDescription.Should().BeNull();
        product.UnitOfMeasure.Should().Be(UnitOfMeasure.License);
        product.UnitPrice.CurrencyCode.Value.Should().Be("EUR");
        product.TaxRate.Value.Should().Be(0m);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        var product = CreateProduct(Guid.CreateVersion7(), Guid.CreateVersion7());

        Action act = () => InvoiceProductSnapshot.Create(
            Guid.Empty, product.ProductId, product.IsActive, product.ProductCode, product.ProductName,
            product.ProductDescription, product.UnitOfMeasure, product.UnitPrice, product.TaxRate);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyProductId()
    {
        var product = CreateProduct(Guid.CreateVersion7(), Guid.CreateVersion7());

        Action act = () => InvoiceProductSnapshot.Create(
            product.ProductTenantId, Guid.Empty, product.IsActive, product.ProductCode, product.ProductName,
            product.ProductDescription, product.UnitOfMeasure, product.UnitPrice, product.TaxRate);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Create_RejectsUndefinedUnitOfMeasure()
    {
        var product = CreateProduct(Guid.CreateVersion7(), Guid.CreateVersion7());

        Action act = () => InvoiceProductSnapshot.Create(
            product.ProductTenantId, product.ProductId, product.IsActive, product.ProductCode, product.ProductName,
            product.ProductDescription, (UnitOfMeasure)999, product.UnitPrice, product.TaxRate);

        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.UnitOfMeasureInvalid);
    }

    private static InvoiceProductSnapshot CreateProduct(Guid tenantId, Guid productId) => InvoiceProductSnapshot.Create(
        tenantId, productId, true,
        ProductCode.Create("PROD-001"), ProductName.Create("Consulting"),
        ProductDescription.Create("Professional services"), UnitOfMeasure.Hour,
        Money.Create(125.50m, CurrencyCode.Create("RON")), Percentage.Create(19m));
}
