using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError UnitOfMeasureInvalid = new("unitOfMeasure.invalid", "Unit of measure is invalid.");
    public static readonly DomainError UpdatedAtUtcRequired = new("updatedAtUtc.required", "Updated datetime is required.");
    public static readonly DomainError UpdatedAtUtcInvalid = new("updatedAtUtc.invalid", "Updated datetime cannot be earlier than Created datetime.");
    public static readonly DomainError TenantIdRequired = new("tenantId.required", "Tenant Id is required.");
    public static readonly DomainError IdRequired = new("id.required", "Id is required.");
    public static readonly DomainError InvoiceIdRequired = new("invoiceId.required", "Invoice Id is required.");
    public static readonly DomainError CreatedAtUtcRequired = new("createdAtUtc.required", "Created datetime is required.");
    public static readonly DomainError MoneyInvalid = new("money.invalid", "Money amount cannot be negative.");
    public static readonly DomainError MoneyDecimalPlacesInvalid = new("moneyDecimalPlaces.invalid", "Money amount does not match the currency precision for the specified currency.");
    public static readonly DomainError PercentageInvalid = new("percentage.invalid", "Percentage must be between 0 and 100.");
    public static readonly DomainError QuantityInvalid = new("quantity.invalid", "Quantity must be greater than zero.");
    public static readonly DomainError InvoiceLineDescriptionRequired = new("invoiceLineDescription.required", "Invoice line description is required.");
    public static readonly DomainError InvoiceLineDescriptionTooLong = new("invoiceLineDescription.tooLong", $"Invoice line description is too long. Maximum length is {InvoiceLineDescription.MaxLength} characters.");
    public static readonly DomainError UnitPriceRequired = new("unitPrice.required", "Unit price is required.");
    public static readonly DomainError ProductRequired = new("product.required", "Product is required.");
    public static readonly DomainError ProductInactive = new("product.inactive", "An inactive product cannot be added to an invoice.");
    public static readonly DomainError CurrencyMismatch = new("currency.mismatch", "Money currency must match the invoice line currency.");
    public static readonly DomainError InvoiceNumberRequired = new("invoiceNumber.required", "Invoice number is required.");
    public static readonly DomainError InvoiceNumberTooLong = new("invoiceNumber.tooLong", $"Invoice number cannot exceed {InvoiceNumber.MaxLength} characters.");
    public static readonly DomainError InvoiceNotesRequired = new("notes.required", "Invoice notes are required.");
    public static readonly DomainError InvoiceNotesTooLong = new("notes.tooLong", $"Invoice notes cannot exceed {InvoiceNotes.MaxLength} characters.");
    public static readonly DomainError TenantMismatch = new($"tenant.mismatch", $"Entities belongs to different tenants.");
    public static readonly DomainError CurrencyHasLines = new($"currency.hasLines", "Invoice currency cannot be changed after lines have been added.");
    public static readonly DomainError PositionInvalid = new("position.invalid", $"Line position must be between 1 and NumberOfLines.");
    public static readonly DomainError IssuedAtUtcRequired = new($"issuedAtUtc.required", $"Invoice issued datetime is required.");
    public static readonly DomainError NoLines = new("invoice.noLines", "An invoice must contain at least one line before it can be issued.");
    public static readonly DomainError GrossAmountZero = new("grossAmount.zeroAmount", "An invoice total must be greater than zero before it can be issued.");
    public static readonly DomainError InvoiceNotIssued = new("invoice.notIssued", "Only issued invoices can receive, reverse payments or can be cancelled.");
    public static readonly DomainError InvoiceNotDraft = new("invoice.notDraft", "Only draft invoices can be modified.");
    public static readonly DomainError CancelledAtUtcRequired = new("cancelledAtUtc.required", "Invoice cancelled datetime is required.");
    public static readonly DomainError InvoicePaidAmountInvalid = new("invoice.paidAmountInvalid", "An invoice with allocated payments cannot be cancelled. Reverse its payment allocations first.");
    public static readonly DomainError PaymentAmountInvalid = new("invoice.amountInvalid", "Payment amount must be greater than zero.");
    public static readonly DomainError PaymentAmountExceedsOutstanding = new("invoice.amountExceedsOutstanding", "Payment amount cannot exceed the outstanding invoice amount.");
    public static readonly DomainError PaymentAmountExceedsPaidAmount = new("invoice.amountExceedsPaidAmount", "Payment amount cannot exceed the invoice paid amount.");
    public static readonly DomainError LineIdInvalid = new("lineId.invalid", "Invoice line Id cannot be empty.");
    public static readonly DomainError LineIdNotFound = new("lineId.notFound", "Invoice line was not found.");
    public static readonly DomainError LineIdDuplicate = new("lineId.duplicate", "An invoice line with the same Id already exists.");
    public static readonly DomainError IssueDateRequired = new("issueDate.required", "Invoice issue date is required.");
    public static readonly DomainError DueDateRequired = new("dueDate.required", "Invoice due date is required.");
    public static readonly DomainError DueDateInvalid = new("dueDate.beforeIssueDate", "Invoice due date cannot be earlier than its issue date.");
    public static readonly DomainError PrefixRequired = new("sequencePrefix.required", "Invoice number prefix is required.");
    public static readonly DomainError YearInvalid = new("sequenceYear.invalid", "Invoice number year is invalid, must be between 2000 and 2100.");
    public static readonly DomainError SequenceNumberInvalid = new("sequenceNumber.invalid", "Invoice sequence number sequence is invalid, must be a positive integer.");
    public static readonly DomainError SequenceNumberExceededMaxValue = new("sequenceNumber.exceededMaxValue", "Invoice sequence number has exceeded its maximum value.");
}
