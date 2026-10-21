using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Invoices.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError InvoiceNotFound = new(ApplicationErrorType.NotFound, "invoice.notFound", "Invoice not found.");
    public static readonly ApplicationError InvoiceCannotBeIssued = new(ApplicationErrorType.Conflict, "invoice.cannotBeIssued", "Invoice cannot be issued.");
}
