using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError CurrencyCodeRequired = new("currencyCode.required", "Currency code is required.");
    public static readonly DomainError CurrencyCodeInvalid = new("currencyCode.invalid", "Currency code is invalid. Expected a valid ISO 4217 currency code.");
    public static readonly DomainError InvoiceNumberRequired = new("invoiceNumber.required", "Invoice number is required.");
    public static readonly DomainError InvoiceNumberTooLong = new("invoiceNumber.tooLong", $"Invoice number cannot exceed {InvoiceNumber.MaxLength} characters.");
    public static readonly DomainError MoneyInvalid = new("money.invalid", "Money amount cannot be negative.");
    public static readonly DomainError MoneyDecimalPlacesInvalid = new("moneyDecimalPlaces.invalid", "Money amount does not match the currency precision for the specified currency.");
    public static readonly DomainError TenantIdRequired = new("tenantId.required", "Tenant Id is required.");
    public static readonly DomainError IdRequired = new("id.required", "Payment allocation Id is required.");
    public static readonly DomainError PaymentIdRequired = new("paymentId.required", "Payment Id is required.");
    public static readonly DomainError InvoiceIdRequired = new("invoiceId.required", "Invoice Id is required.");
    public static readonly DomainError AllocationAmountInvalid = new("amount.invalid", "Payment allocation amount must be greater than zero.");
    public static readonly DomainError CreatedAtUtcInvalid = new("createdAtUtc.invalid", "Payment allocation created datetime is required.");
    public static readonly DomainError CreatedAtUtcNotUtc = new("createdAtUtc.notUtc", "Payment allocation created datetime must be UTC.");
    public static readonly DomainError PaymentNumberRequired = new("number.required", "Payment number is required.");
    public static readonly DomainError PaymentNumberTooLong = new("number.tooLong", $"Payment number cannot exceed {PaymentNumber.MaxLength} characters.");
    public static readonly DomainError PaymentReferenceRequired = new("reference.required", "Payment reference is required.");
    public static readonly DomainError PaymentReferenceTooLong = new("reference.tooLong", $"Payment reference cannot exceed {PaymentReference.MaxLength} characters.");
    public static readonly DomainError PaymentNotesRequired = new("payment.notes.required", "Payment notes is required.");
    public static readonly DomainError PaymentNotesTooLong = new("payment.notes.tooLong", $"Payment notes cannot exceed {PaymentNotes.MaxLength} characters.");
    public static readonly DomainError AllocationIdInvalid = new("allocationId.invalid", "A payment allocation with the same Id already exists.");
    public static readonly DomainError InvoiceIdInvalid = new("invoiceId.invalid", "This payment already has an allocation for the specified invoice.");
    public static readonly DomainError AmountExceedsUnallocated = new("amount.exceedsUnallocated", "Payment allocation amount cannot exceed the unallocated payment amount.");
    public static readonly DomainError PostedAtUtcRequired = new("postedAtUtc.required", "Payment posted datetime is required.");
    public static readonly DomainError PostedAtUtcNotUtc = new("postedAtUtc.notUtc", "Payment posted datetime must be UTC.");
    public static readonly DomainError NoAllocations = new("payment.noAllocations", "A payment must contain at least one allocation before it can be posted.");
    public static readonly DomainError AllocatedAmountInvalid = new("payment.allocatedAmount.invalid", "The allocated payment amount must be greater than zero.");
    public static readonly DomainError NotFullyAllocated = new("payment.notFullyAllocated", "The entire payment amount must be allocated before posting.");
    public static readonly DomainError CanNotBeCancelled = new("payment.canNotBeCancelled", "Only a posted payment can be cancelled.");
    public static readonly DomainError CancelledAtUtcRequired = new("cancelledAtUtc.required", "Payment cancelled datetime is required.");
    public static readonly DomainError CancelledAtUtcNotUtc = new("cancelledAtUtc.notUtc", "Payment cancelled datetime must be UTC.");
    public static readonly DomainError AllocationNotFound = new("allocation.notFound", "Payment allocation was not found.");
    public static readonly DomainError CanNotBeModified = new("payment.canNotBeModified", "Only draft payments can be modified.");
    public static readonly DomainError UpdatedAtUtcRequired = new("updatedAtUtc.required", "Payment updated datetime is required.");
    public static readonly DomainError UpdatedAtUtcNotUtc = new("updatedAtUtc.notUtc", "Payment updated datetime must be UTC.");
    public static readonly DomainError UpdatedAtUtcInvalid = new("updatedAtUtc.invalid", "Payment updated cannot be earlier than its created datetime or its previous updated datetime.");
    public static readonly DomainError CustomerIdRequired = new("payment.customerId.required", "Customer Id is required.");
    public static readonly DomainError AmountInvalid = new("amount.invalid", "Payment amount must be greater than zero.");
    public static readonly DomainError CurrencyMismatch = new("currency.mismatch", "Money currency must match the invoice line currency.");
    public static readonly DomainError MethodInvalid = new("paymentMethod.invalid", "Payment method is invalid.");
    public static readonly DomainError PaymentDateRequired = new("paymentDate.required", "Payment date is required.");
}
