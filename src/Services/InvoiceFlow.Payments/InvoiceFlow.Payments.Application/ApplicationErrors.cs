using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Payments.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError IdempotencyKeyConflict = new(ApplicationErrorType.Conflict, "idempotencyKey.conflict", "A request with this idempotency key is already being processed.");
    public static readonly ApplicationError TransientFailure = new(ApplicationErrorType.ServiceUnavailable, "persistence.transient_failure", "The operation may not have completed because of a temporary service failure. Retry with the same idempotency key when one was supplied.");
    public static readonly ApplicationError RequestHashInputRequired = new(ApplicationErrorType.Validation, "Idempotency.RequestHashInputRequired", "A request payload is required to calculate its idempotency hash.");
    public static readonly ApplicationError PaymentNotFound = new(ApplicationErrorType.NotFound, "payment.notFound", "Payment not found.");


    public static readonly ApplicationError PaymentCannotBePosted = new(ApplicationErrorType.Conflict, "payment.cannotBePosted", "Payment cannot be posted.");
    public static readonly ApplicationError IdempotencyKeyReused = new(ApplicationErrorType.Conflict, "idempotencyKey.reused", "Idempotency key already used.");
    public static readonly ApplicationError IdempotencyResourceNotFound = new(ApplicationErrorType.Conflict, "idempotencyResource.notFound", "The resource associated with this idempotency key could not be found.");
    public static readonly ApplicationError CustomerNotFound = new(ApplicationErrorType.NotFound, "customer.notFound", "The specified customer could not be found.");
}
