using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError IdempotencyOperationInvalid = new("idempotencyOperation.invalid", "idempotency operation is invalid.");
    public static readonly DomainError IdempotencyKeyRequired = new("idempotencyKey.required", "idempotency key is required.");
    public static readonly DomainError IdempotencyKeyTooLong = new("idempotencyKey.tooLong", $"idempotency key cannot exceed {IdempotencyKey.MaxLength} characters.");
    public static readonly DomainError RequestHashRequired = new("requestHash.required", "Request hash prefix is required.");
    public static readonly DomainError RequestHashInvalid = new("requestHash.invalid", "The request hash must be a valid 64-character SHA-256 hexadecimal value.");
    public static readonly DomainError ResourceIdRequired = new("resourceId.required", "Resource Id is required.");
}
