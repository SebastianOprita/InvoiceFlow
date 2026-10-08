using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Application;

public static class ApplicationErrors
{
    public static readonly ApplicationError ConcurrencyConflict = new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static readonly ApplicationError CustomerNotFound = new(ApplicationErrorType.NotFound, "customer.notFound", "Customer not found.");
    public static readonly ApplicationError CustomerCodeAlreadyExists = new(ApplicationErrorType.Conflict, "customerCode.alreadyExists", "Customer with the same code already exists.");
    public static readonly ApplicationError CustomerRegistrationNumberAlreadyExists = new(ApplicationErrorType.Conflict, "customerRegistrationNumber.alreadyExists", "Customer with the same registration number already exists.");
    public static readonly ApplicationError CustomerTaxNumberAlreadyExists = new(ApplicationErrorType.Conflict, "customerTaxNumber.alreadyExists", "Customer with the same tax number already exists.");
}
