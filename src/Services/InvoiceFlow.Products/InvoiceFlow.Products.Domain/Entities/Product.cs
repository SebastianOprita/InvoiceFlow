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
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

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
        DateTime createdAtUtc)
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
        DateTime createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

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
        DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        Name = name;
        Description = description;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeCode(
        ProductCode code,
        DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (Code == code)
            return;

        Code = code;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeUnitOfMeasure(
        UnitOfMeasure unitOfMeasure,
        DateTime updatedAtUtc)
    {
        ValidateUnitOfMeasure(unitOfMeasure);
        EnsureValidUpdateTime(updatedAtUtc);

        if (UnitOfMeasure == unitOfMeasure)
            return;

        UnitOfMeasure = unitOfMeasure;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeUnitPrice(
        Money unitPrice,
        DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (UnitPrice == unitPrice)
            return;

        UnitPrice = unitPrice;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeTaxRate(
        Percentage taxRate,
        DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (TaxRate == taxRate)
            return;

        TaxRate = taxRate;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void UpdateSalesInformation(
        UnitOfMeasure unitOfMeasure,
        Money unitPrice,
        Percentage taxRate,
        DateTime updatedAtUtc)
    {
        ValidateUnitOfMeasure(unitOfMeasure);
        EnsureValidUpdateTime(updatedAtUtc);

        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        EnsureValidUpdateTime(updatedAtUtc);

        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
    {
        if (!Enum.IsDefined(unitOfMeasure))
            throw new DomainException(DomainErrors.UnitOfMeasureInvalid);
    }

    private void EnsureValidUpdateTime(DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        if (updatedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.UpdatedAtUtcNotUtc);

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);
    }
}
