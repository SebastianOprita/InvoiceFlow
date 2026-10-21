using System.Reflection;

namespace InvoiceFlow.Products.ArchitectureTests;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Domain =
        Assembly.Load("InvoiceFlow.Products.Domain");

    public static readonly Assembly Application =
        Assembly.Load("InvoiceFlow.Products.Application");

    public static readonly Assembly Infrastructure =
        Assembly.Load("InvoiceFlow.Products.Infrastructure");

    public static readonly Assembly Api =
        Assembly.Load("InvoiceFlow.Products.Api");
}
