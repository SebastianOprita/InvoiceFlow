using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Payments.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError IdempotencyKeyConflict = new(ApplicationErrorType.Conflict, "idempotencyKey.conflict", "A request with this idempotency key is already being processed.");
    public static readonly ApplicationError TransientFailure = new(ApplicationErrorType.ServiceUnavailable, "persistence.transient_failure", "The operation may not have completed because of a temporary service failure. Retry with the same idempotency key when one was supplied.");
    public static readonly ApplicationError RequestHashInputRequired = new(ApplicationErrorType.Validation, "Idempotency.RequestHashInputRequired", "A request payload is required to calculate its idempotency hash.");
    public static readonly ApplicationError PaymentNotFound = new(ApplicationErrorType.NotFound, "payment.notFound", "Payment not found.");
    public static readonly ApplicationError InvoicePaymentBalanceConflict = new(ApplicationErrorType.Conflict, "invoicePaymentBalance.conflict", "The invoice payment balance already exists with different source values.");

    public static readonly ApplicationError InvoicePaymentBalanceNotFound = new(ApplicationErrorType.NotFound, "invoicePaymentBalance.notFound", "Invoice payment balance not found.");
    public static readonly ApplicationError InsufficientUnallocatedPaymentAmount = new(ApplicationErrorType.Conflict, "payment.insufficientUnallocatedAmount", "Payment does not have enough available money to pay the invoice amount.");
    public static readonly ApplicationError PaymentMustBePosted = new(ApplicationErrorType.Conflict, "payment.mustBePosted", "A payment must be posted before it can be allocated.");
    public static readonly ApplicationError AllocationAmountPrecisionInvalid = new(ApplicationErrorType.Validation, "paymentAllocation.amountPrecisionInvalid", "Allocation amount has more decimal places than the invoice currency allows.");
    public static readonly ApplicationError AllocationAmountExceedsInvoiceOutstanding = new(ApplicationErrorType.Conflict, "paymentAllocation.amountExceedsOutstanding", "Allocation amount cannot exceed the invoice's outstanding amount.");
    public static readonly ApplicationError PaymentCurrencyMismatch = new(ApplicationErrorType.Conflict, "payment.currencyMismatch", "Payment currency must match the invoice currency.");

    public static readonly ApplicationError PaymentCannotBePosted = new(ApplicationErrorType.Conflict, "payment.cannotBePosted", "Payment cannot be posted.");
    public static readonly ApplicationError IdempotencyKeyReused = new(ApplicationErrorType.Conflict, "idempotencyKey.reused", "Idempotency key already used.");
    public static readonly ApplicationError IdempotencyResourceNotFound = new(ApplicationErrorType.Conflict, "idempotencyResource.notFound", "The resource associated with this idempotency key could not be found.");
    public static readonly ApplicationError CustomerNotFound = new(ApplicationErrorType.NotFound, "customer.notFound", "The specified customer could not be found.");
}
