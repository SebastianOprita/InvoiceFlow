using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.Entities;

public sealed class InvoiceLineTests
{
    [Fact]
    public void CreateFromProduct_CopiesProductDataAndCalculatesAmounts()
    {
        var product = InvoiceEntityTestData.Product(description: ProductDescription.Create("Consulting"));
        var line = InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            product, Quantity.Create(2), Percentage.Create(10));

        line.TenantId.Should().Be(InvoiceEntityTestData.TenantId);
        line.Position.Should().Be(1);
        line.ProductId.Should().Be(product.ProductId);
        line.ProductCode.Should().Be(product.ProductCode);
        line.Description.Value.Should().Be("Consulting");
        line.DiscountAmount.Amount.Should().Be(20m);
        line.NetAmount.Amount.Should().Be(180m);
        line.TaxAmount.Amount.Should().Be(34.20m);
        line.GrossAmount.Amount.Should().Be(214.20m);
    }

    [Fact]
    public void CreateFromProduct_UsesProductNameWhenDescriptionIsNull()
    {
        var line = InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);

        line.Description.Value.Should().Be("Service");
    }

    [Fact]
    public void CreateManualLine_HasNoProductReference()
    {
        var line = InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceLineDescription.Create("Manual"), Quantity.Create(1), UnitOfMeasure.Hour,
            Money.Create(50m, InvoiceEntityTestData.RON), Percentage.Zero, Percentage.Zero);

        line.ProductId.Should().BeNull();
        line.ProductCode.Should().BeNull();
        line.GrossAmount.Amount.Should().Be(50m);
    }

    [Fact]
    public void Create_RejectsEmptyTenantId()
    {
        Action act = () => InvoiceLine.Create(Guid.Empty, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyLineId()
    {
        Action act = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.Empty, Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Create_RejectsEmptyInvoiceId()
    {
        Action act = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.Empty, 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.InvoiceIdRequired);
    }

    [Fact]
    public void Create_RejectsInvalidPosition()
    {
        Action act = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 0,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        act.Should().Throw<DomainException>().WithDomainError(DomainErrors.PositionInvalid);
    }

    [Fact]
    public void CreateFromProduct_RejectsMissingInactiveOrForeignProduct()
    {
        Action missing = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            null!, Quantity.Create(1), Percentage.Zero);
        Action inactive = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(active: false), Quantity.Create(1), Percentage.Zero);
        Action foreign = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(Guid.CreateVersion7()), Quantity.Create(1), Percentage.Zero);
        missing.Should().Throw<DomainException>().WithDomainError(DomainErrors.ProductRequired);
        inactive.Should().Throw<DomainException>().WithDomainError(DomainErrors.ProductInactive);
        foreign.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantMismatch);
    }

    [Fact]
    public void CreateManualLine_RejectsInvalidUnitAndMissingPrice()
    {
        Action invalidUnit = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceLineDescription.Create("x"), Quantity.Create(1), (UnitOfMeasure)999,
            Money.Create(1m, InvoiceEntityTestData.RON), Percentage.Zero, Percentage.Zero);
        Action missingPrice = () => InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceLineDescription.Create("x"), Quantity.Create(1), UnitOfMeasure.Service,
            null!, Percentage.Zero, Percentage.Zero);
        invalidUnit.Should().Throw<DomainException>().WithDomainError(DomainErrors.UnitOfMeasureInvalid);
        missingPrice.Should().Throw<DomainException>().WithDomainError(DomainErrors.UnitPriceRequired);
    }

    [Fact]
    public void Update_RecalculatesLineAndRejectsCurrencyMismatch()
    {
        var line = CreateManualLine();
        line.Update(InvoiceLineDescription.Create("Updated"), Quantity.Create(2), UnitOfMeasure.Hour,
            Money.Create(25m, InvoiceEntityTestData.RON), Percentage.Create(20), Percentage.Create(10));

        line.Description.Value.Should().Be("Updated");
        line.DiscountAmount.Amount.Should().Be(10m);
        line.NetAmount.Amount.Should().Be(40m);
        line.TaxAmount.Amount.Should().Be(4m);
        line.GrossAmount.Amount.Should().Be(44m);

        Action mismatch = () => line.Update(InvoiceLineDescription.Create("USD"), Quantity.Create(1), UnitOfMeasure.Hour,
            Money.Create(10m, CurrencyCode.Create("USD")), Percentage.Zero, Percentage.Zero);
        mismatch.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Fact]
    public void ReplaceProduct_UpdatesProductAndRejectsInvalidProducts()
    {
        var line = InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        var product = InvoiceEntityTestData.Product(description: ProductDescription.Create("Replacement"), price: 75m);
        line.ReplaceProduct(product);
        line.ProductId.Should().Be(product.ProductId);
        line.Description.Value.Should().Be("Replacement");
        line.UnitPrice.Amount.Should().Be(75m);

        Action foreign = () => line.ReplaceProduct(InvoiceEntityTestData.Product(Guid.CreateVersion7()));
        Action inactive = () => line.ReplaceProduct(InvoiceEntityTestData.Product(active: false));
        Action wrongCurrency = () => line.ReplaceProduct(InvoiceEntityTestData.Product(currency: CurrencyCode.Create("USD")));
        foreign.Should().Throw<DomainException>().WithDomainError(DomainErrors.TenantMismatch);
        inactive.Should().Throw<DomainException>().WithDomainError(DomainErrors.ProductInactive);
        wrongCurrency.Should().Throw<DomainException>().WithDomainError(DomainErrors.CurrencyMismatch);
    }

    [Fact]
    public void RemoveProductReference_ClearsProductFields()
    {
        var line = InvoiceLine.Create(InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
            InvoiceEntityTestData.Product(), Quantity.Create(1), Percentage.Zero);
        line.RemoveProductReference();
        line.ProductId.Should().BeNull();
        line.ProductCode.Should().BeNull();
    }

    [Fact]
    public void ChangePosition_RequiresPositivePosition()
    {
        var line = CreateManualLine();
        line.ChangePosition(3);
        line.Position.Should().Be(3);
        Action invalid = () => line.ChangePosition(0);
        invalid.Should().Throw<DomainException>().WithDomainError(DomainErrors.PositionInvalid);
    }

    private static InvoiceLine CreateManualLine() => InvoiceLine.Create(
        InvoiceEntityTestData.TenantId, Guid.CreateVersion7(), Guid.CreateVersion7(), 1,
        InvoiceLineDescription.Create("Manual"), Quantity.Create(1), UnitOfMeasure.Service,
        Money.Create(100m, InvoiceEntityTestData.RON), Percentage.Zero, Percentage.Zero);
}
