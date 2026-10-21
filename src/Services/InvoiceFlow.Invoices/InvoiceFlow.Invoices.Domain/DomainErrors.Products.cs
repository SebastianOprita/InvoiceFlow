using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError ProductCodeRequired = new("productCode.required", "Product code is required.");
    public static readonly DomainError ProductCodeTooLong = new("productCode.tooLong", $"Product code cannot exceed {ProductCode.MaxLength} characters.");
    public static readonly DomainError ProductNameRequired = new("productName.required", "Product name is required.");
    public static readonly DomainError ProductNameTooLong = new("productName.tooLong", $"Product name cannot exceed {ProductName.MaxLength} characters.");
    public static readonly DomainError ProductDescriptionRequired = new("productDescription.required", "Product description is required.");
    public static readonly DomainError ProductDescriptionTooLong = new("productDescription.tooLong", $"Product description cannot exceed {ProductDescription.MaxLength} characters.");
}
