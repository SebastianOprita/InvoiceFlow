using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Invoices.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError IdempotencyKeyConflict = new(ApplicationErrorType.Conflict, "idempotencyKey.conflict", "A request with this idempotency key is already being processed.");
    public static readonly ApplicationError InvoiceNotFound = new(ApplicationErrorType.NotFound, "invoice.notFound", "Invoice not found.");
    public static readonly ApplicationError InvoiceCannotBeIssued = new(ApplicationErrorType.Conflict, "invoice.cannotBeIssued", "Invoice cannot be issued.");
    public static readonly ApplicationError IdempotencyKeyReused = new(ApplicationErrorType.Conflict, "idempotencyKey.reused", "Idempotency key already used.");
    public static readonly ApplicationError IdempotencyResourceNotFound = new(ApplicationErrorType.Conflict, "idempotencyResource.notFound", "The resource associated with this idempotency key could not be found.");
    public static readonly ApplicationError CustomerNotFound = new(ApplicationErrorType.NotFound, "customer.notFound", "The specified customer could not be found.");
    public static readonly ApplicationError ProductNotFound = new(ApplicationErrorType.NotFound, "product.notFound", "The specified product could not be found.");
    public static readonly ApplicationError CustomerInactive = new(ApplicationErrorType.Conflict, "customer.inactive", "An inactive customer cannot be used to create an invoice.");
    public static readonly ApplicationError ProductInactive = new(ApplicationErrorType.Conflict, "product.inactive", "An inactive product cannot be added to an invoice.");
    public static readonly ApplicationError RequestHashInputRequired = new(ApplicationErrorType.Validation, "Idempotency.RequestHashInputRequired", "A request payload is required to calculate its idempotency hash.");
    public static readonly ApplicationError TransientFailure = new(ApplicationErrorType.ServiceUnavailable, "persistence.transient_failure", "The operation may not have completed because of a temporary service failure. Retry with the same idempotency key when one was supplied.");
}
