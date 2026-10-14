using System.Reflection;

namespace InvoiceFlow.Customers.ArchitectureTests;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Domain =
        Assembly.Load("InvoiceFlow.Customers.Domain");

    public static readonly Assembly Application =
        Assembly.Load("InvoiceFlow.Customers.Application");

    public static readonly Assembly Infrastructure =
        Assembly.Load("InvoiceFlow.Customers.Infrastructure");

    public static readonly Assembly Api =
        Assembly.Load("InvoiceFlow.Customers.Api");
}
