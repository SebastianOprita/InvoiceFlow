using InvoiceFlow.Common.Application;

namespace InvoiceFlow.Customers.Application;

public static class ApplicationErrors
{
    public static Error CustomerNotFound => new(ErrorType.NotFound, "customer.notFound", "Customer not found.");
}
