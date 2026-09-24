using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Application;

public static class ApplicationErrors
{
    public static ApplicationError CustomerNotFound => new(ApplicationErrorType.NotFound, "customer.notFound", "Customer not found.");
    public static ApplicationError ActivateCustomerNotFound => new(ApplicationErrorType.NotFound, "activateCustomer.customer.notFound", "Customer not found.");
    public static ApplicationError CreateCustomerCodeAlreadyExists => new(ApplicationErrorType.Conflict, "createCustomer.customer.code.alreadyExists", "Customer with the same code already exists.");
    public static ApplicationError CreateCustomerRegistrationNumberAlreadyExists => new(ApplicationErrorType.Conflict, "createCustomer.customer.registrationNumber.alreadyExists", "Customer with the same registration number already exists.");
    public static ApplicationError CreateCustomerTaxNumberAlreadyExists => new(ApplicationErrorType.Conflict, "createCustomer.customer.taxNumber.alreadyExists", "Customer with the same tax number already exists.");
    public static ApplicationError DeactivateCustomerNotFound => new(ApplicationErrorType.NotFound, "deactivateCustomer.customer.notFound", "Customer not found.");
    public static ApplicationError UpdateCustomerNotFound => new(ApplicationErrorType.NotFound, "updateCustomer.customer.notFound", "Customer not found.");
}
