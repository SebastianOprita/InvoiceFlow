using System.Reflection;

namespace InvoiceFlow.Invoices.ArchitectureTests;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Domain =
        Assembly.Load("InvoiceFlow.Invoices.Domain");

    public static readonly Assembly Application =
        Assembly.Load("InvoiceFlow.Invoices.Application");

    public static readonly Assembly Infrastructure =
        Assembly.Load("InvoiceFlow.Invoices.Infrastructure");

    public static readonly Assembly Api =
        Assembly.Load("InvoiceFlow.Invoices.Api");
}
