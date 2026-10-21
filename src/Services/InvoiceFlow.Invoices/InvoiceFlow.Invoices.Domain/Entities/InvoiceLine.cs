using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed class InvoiceLine
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public int Position { get; private set; }
    public Guid? ProductId { get; private set; }
    public ProductCode? ProductCode { get; private set; }
    public InvoiceLineDescription Description { get; private set; }
    public Quantity Quantity { get; private set; }
    public UnitOfMeasure UnitOfMeasure { get; private set; }
    public Money UnitPrice { get; private set; }
    public Percentage DiscountRate { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Percentage TaxRate { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money NetAmount { get; private set; }
    public Money GrossAmount { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

#pragma warning disable CS8618
    private InvoiceLine() { }
#pragma warning restore CS8618

    private InvoiceLine(
        Guid tenantId,
        Guid id,
        Guid invoiceId,
        int position,
        Guid? productId,
        ProductCode? productCode,
        InvoiceLineDescription description,
        Quantity quantity,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage discountRate,
        Percentage taxRate)
    {
        TenantId = tenantId;
        Id = id;
        InvoiceId = invoiceId;
        Position = position;
        ProductId = productId;
        ProductCode = productCode;
        Description = description;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        DiscountRate = discountRate;
        TaxRate = taxRate;

        DiscountAmount = Money.Zero(unitPrice.CurrencyCode);
        NetAmount = Money.Zero(unitPrice.CurrencyCode);
        TaxAmount = Money.Zero(unitPrice.CurrencyCode);
        GrossAmount = Money.Zero(unitPrice.CurrencyCode);

        Recalculate();
    }

    public static InvoiceLine Create(
        Guid tenantId,
        Guid id,
        Guid invoiceId,
        int position,
        InvoiceProductSnapshot product,
        Quantity quantity,
        Percentage discountRate)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateInvoiceId(invoiceId);
        ValidatePosition(position);

        if (product is null)
            throw new DomainException(DomainErrors.ProductRequired);

        if (product.ProductTenantId != tenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!product.IsActive)
            throw new DomainException(DomainErrors.ProductInactive);

        ValidateUnitOfMeasure(product.UnitOfMeasure);
        ValidateMoney(product.UnitPrice);

        return new InvoiceLine(
            tenantId,
            id,
            invoiceId,
            position,
            product.ProductId,
            product.ProductCode,
            InvoiceLineDescription.Create(product.ProductDescription?.Value ?? product.ProductName.Value),
            quantity,
            product.UnitOfMeasure,
            product.UnitPrice,
            discountRate,
            product.TaxRate);
    }

    public static InvoiceLine Create(
        Guid tenantId,
        Guid id,
        Guid invoiceId,
        int position,
        InvoiceLineDescription description,
        Quantity quantity,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage discountRate,
        Percentage taxRate)
    {
        ValidateTenantId(tenantId);
        ValidateId(id);
        ValidateInvoiceId(invoiceId);
        ValidatePosition(position);
        ValidateUnitOfMeasure(unitOfMeasure);
        ValidateMoney(unitPrice);

        return new InvoiceLine(
            tenantId,
            id,
            invoiceId,
            position,
            null,
            null,
            description,
            quantity,
            unitOfMeasure,
            unitPrice,
            discountRate,
            taxRate);
    }

    public void Update(
        InvoiceLineDescription description,
        Quantity quantity,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage discountRate,
        Percentage taxRate)
    {
        ValidateUnitOfMeasure(unitOfMeasure);
        ValidateMoney(unitPrice);
        EnsureCurrencyMatches(unitPrice);

        Description = description;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        DiscountRate = discountRate;
        TaxRate = taxRate;

        Recalculate();
    }

    public void ReplaceProduct(InvoiceProductSnapshot product)
    {
        if (product is null)
            throw new DomainException(DomainErrors.ProductRequired);

        if (product.ProductTenantId != TenantId)
            throw new DomainException(DomainErrors.TenantMismatch);

        if (!product.IsActive)
            throw new DomainException(DomainErrors.ProductInactive);

        ValidateUnitOfMeasure(product.UnitOfMeasure);
        ValidateMoney(product.UnitPrice);
        EnsureCurrencyMatches(product.UnitPrice);

        ProductId = product.ProductId;
        ProductCode = product.ProductCode;
        Description = InvoiceLineDescription.Create(product.ProductDescription?.Value ?? product.ProductName.Value);
        UnitOfMeasure = product.UnitOfMeasure;
        UnitPrice = product.UnitPrice;
        TaxRate = product.TaxRate;

        Recalculate();
    }

    public void RemoveProductReference()
    {
        ProductId = null;
        ProductCode = null;
    }

    public void ChangePosition(int position)
    {
        ValidatePosition(position);

        Position = position;
    }

    private void EnsureCurrencyMatches(Money money)
    {
        if (money.CurrencyCode != UnitPrice.CurrencyCode)
            throw new DomainException(DomainErrors.CurrencyMismatch);
    }

    private void Recalculate()
    {
        var currencyCode = UnitPrice.CurrencyCode;

        var baseAmount = Quantity.Value * UnitPrice.Amount;

        var discountAmount = RoundMoney(baseAmount * DiscountRate.Value / 100m, currencyCode);

        var netAmount = RoundMoney(baseAmount - discountAmount, currencyCode);

        var taxAmount = RoundMoney(netAmount * TaxRate.Value / 100m, currencyCode);

        var grossAmount = RoundMoney(netAmount + taxAmount, currencyCode);

        DiscountAmount = Money.Create(discountAmount, currencyCode);

        NetAmount = Money.Create(netAmount, currencyCode);

        TaxAmount = Money.Create(taxAmount, currencyCode);

        GrossAmount = Money.Create(grossAmount, currencyCode);
    }
    private static decimal RoundMoney(decimal amount, CurrencyCode currencyCode)
    {
        return decimal.Round(amount, currencyCode.DecimalPlaces, MidpointRounding.AwayFromZero);
    }

    private static void ValidateTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);
    }

    private static void ValidateInvoiceId(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new DomainException(DomainErrors.InvoiceIdRequired);
    }

    private static void ValidatePosition(int position)
    {
        if (position <= 0)
            throw new DomainException(DomainErrors.PositionInvalid);
    }

    private static void ValidateMoney(Money money)
    {
        if (money is null)
            throw new DomainException(DomainErrors.UnitPriceRequired);
    }

    private static void ValidateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
    {
        if (!Enum.IsDefined(unitOfMeasure))
            throw new DomainException(DomainErrors.UnitOfMeasureInvalid);
    }
}
