using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Products.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError ProductNotFound = new(ApplicationErrorType.NotFound, "product.notFound", "Product not found.");
}
