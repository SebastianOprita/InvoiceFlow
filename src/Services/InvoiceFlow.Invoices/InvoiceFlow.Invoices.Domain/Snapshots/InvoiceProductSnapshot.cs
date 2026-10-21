using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceProductSnapshot
{
    public Guid ProductTenantId { get; }
    public Guid ProductId { get; }
    public bool IsActive { get; }
    public ProductCode ProductCode { get; }
    public ProductName ProductName { get; }
    public ProductDescription? ProductDescription { get; }
    public UnitOfMeasure UnitOfMeasure { get; }
    public Money UnitPrice { get; }
    public Percentage TaxRate { get; }

    private InvoiceProductSnapshot(
        Guid productTenantId,
        Guid productId,
        bool isActive,
        ProductCode productCode,
        ProductName productName,
        ProductDescription? productDescription,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate)
    {
        ProductTenantId = productTenantId;
        ProductId = productId;
        IsActive = isActive;
        ProductCode = productCode;
        ProductName = productName;
        ProductDescription = productDescription;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
    }

    public static InvoiceProductSnapshot Create(
        Guid productTenantId,
        Guid productId,
        bool isActive,
        ProductCode productCode,
        ProductName productName,
        ProductDescription? productDescription,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate)
    {
        if (productTenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (productId == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (!Enum.IsDefined(unitOfMeasure))
            throw new DomainException(DomainErrors.UnitOfMeasureInvalid);

        return new InvoiceProductSnapshot(
            productTenantId,
            productId,
            isActive,
            productCode,
            productName,
            productDescription,
            unitOfMeasure,
            unitPrice,
            taxRate);
    }
}
