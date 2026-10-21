using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Products.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError TenantIdRequired = new("tenantId.required", "Tenant Id is required.");
    public static readonly DomainError IdRequired = new("id.required", "Id is required.");
    public static readonly DomainError CreatedAtUtcRequired = new("createdAtUtc.required", "Created datetime is required.");
    public static readonly DomainError CreatedAtUtcNotUtc = new("createdAtUtc.notUtc", "Created datetime must be UTC.");
    public static readonly DomainError UnitOfMeasureInvalid = new("unitOfMeasure.invalid", "Unit of measure is invalid.");
    public static readonly DomainError UpdatedAtUtcRequired = new("updatedAtUtc.required", "Updated datetime is required.");
    public static readonly DomainError UpdatedAtUtcNotUtc = new("updatedAtUtc.notUtc", "Updated datetime must be in UTC.");
    public static readonly DomainError UpdatedAtUtcInvalid = new("updatedAtUtc.invalid", "Updated datetime cannot be earlier than Created datetime.");
    public static readonly DomainError ProductCodeRequired = new("productCode.required", "Product code is required.");
    public static readonly DomainError ProductCodeTooLong = new("productCode.tooLong", $"Product code cannot exceed {ProductCode.MaxLength} characters.");
    public static readonly DomainError ProductNameRequired = new("productName.required", "Product name is required.");
    public static readonly DomainError ProductNameTooLong = new("productName.tooLong", $"Product name cannot exceed {ProductName.MaxLength} characters.");
    public static readonly DomainError ProductDescriptionRequired = new("productDescription.required", "Product description is required.");
    public static readonly DomainError ProductDescriptionTooLong = new("productDescription.tooLong", $"Product description cannot exceed {ProductDescription.MaxLength} characters.");
    public static readonly DomainError PercentageInvalid = new("percentage.invalid", "Percentage must be between 0 and 100.");
    public static readonly DomainError CurrencyCodeRequired = new("currencyCode.required", "Currency code is required.");
    public static readonly DomainError CurrencyCodeInvalid = new("currencyCode.invalid", "Currency code is invalid. Expected a valid ISO 4217 currency code.");
    public static readonly DomainError MoneyInvalid = new("money.invalid", "Money amount cannot be negative.");
    public static readonly DomainError MoneyDecimalPlacesInvalid = new("moneyDecimalPlaces.invalid", "Money amount does not match the currency precision for the specified currency.");
}
