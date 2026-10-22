using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Products.Domain;

public sealed class Product
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public ProductCode Code { get; private set; }
    public ProductName Name { get; private set; }
    public ProductDescription? Description { get; private set; }
    public UnitOfMeasure UnitOfMeasure { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Percentage TaxRate { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Product() { } // EF Core
#pragma warning restore CS8618

    private Product(
        Guid tenantId,
        Guid id,
        ProductCode code,
        ProductName name,
        ProductDescription? description,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate,
        DateTimeOffset createdAtUtc)
    {
        TenantId = tenantId;
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static Product Create(
        Guid tenantId,
        Guid id,
        ProductCode code,
        ProductName name,
        ProductDescription? description,
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        ValidateUnitOfMeasure(unitOfMeasure);

        return new Product(
            tenantId,
            id,
            code,
            name,
            description,
            unitOfMeasure,
            unitPrice,
            taxRate,
            createdAtUtc);
    }

    public void UpdateDetails(
        ProductName name,
        ProductDescription? description,
        DateTimeOffset updatedAtUtc)
    {
        Name = name;
        Description = description;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void ChangeCode(
        ProductCode code,
        DateTimeOffset updatedAtUtc)
    {
        if (Code == code)
            return;

        Code = code;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void ChangeUnitOfMeasure(
        UnitOfMeasure unitOfMeasure,
        DateTimeOffset updatedAtUtc)
    {
        ValidateUnitOfMeasure(unitOfMeasure);

        if (UnitOfMeasure == unitOfMeasure)
            return;

        UnitOfMeasure = unitOfMeasure;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void ChangeUnitPrice(
        Money unitPrice,
        DateTimeOffset updatedAtUtc)
    {
        if (UnitPrice == unitPrice)
            return;

        UnitPrice = unitPrice;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void ChangeTaxRate(
        Percentage taxRate,
        DateTimeOffset updatedAtUtc)
    {
        if (TaxRate == taxRate)
            return;

        TaxRate = taxRate;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void UpdateSalesInformation(
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate,
        DateTimeOffset updatedAtUtc)
    {
        ValidateUnitOfMeasure(unitOfMeasure);

        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = EnsureValidUpdateTime(updatedAtUtc);
    }

    private static void ValidateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
    {
        if (!Enum.IsDefined(unitOfMeasure))
            throw new DomainException(DomainErrors.UnitOfMeasureInvalid);
    }

    private DateTimeOffset EnsureValidUpdateTime(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        updatedAtUtc = updatedAtUtc.ToUniversalTime();

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        return updatedAtUtc;
    }
}
