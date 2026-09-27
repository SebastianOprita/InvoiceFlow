using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Application;

public static class ApplicationErrors
{
    public static ApplicationError ConcurencyConflict => new(ApplicationErrorType.Conflict, "persistence.concurrency_conflict", "The data was modified by another process.");
    public static ApplicationError DbSaveFailed => new(ApplicationErrorType.Internal, "persistence.save_failed", "A database error occurred while saving changes.");
    public static ApplicationError CustomerNotFound => new(ApplicationErrorType.NotFound, "customer.notFound", "Customer not found.");
    public static ApplicationError CustomerCodeAlreadyExists => new(ApplicationErrorType.Conflict, "customer.code.alreadyExists", "Customer with the same code already exists.");
    public static ApplicationError CustomerRegistrationNumberAlreadyExists => new(ApplicationErrorType.Conflict, "customer.registrationNumber.alreadyExists", "Customer with the same registration number already exists.");
    public static ApplicationError CustomerTaxNumberAlreadyExists => new(ApplicationErrorType.Conflict, "customer.taxNumber.alreadyExists", "Customer with the same tax number already exists.");
}
