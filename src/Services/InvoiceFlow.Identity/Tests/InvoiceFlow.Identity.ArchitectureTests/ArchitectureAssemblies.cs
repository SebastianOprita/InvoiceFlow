using System.Reflection;

namespace InvoiceFlow.Identity.ArchitectureTests;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Domain =
        Assembly.Load("InvoiceFlow.Identity.Domain");

    public static readonly Assembly Application =
        Assembly.Load("InvoiceFlow.Identity.Application");

    public static readonly Assembly Infrastructure =
        Assembly.Load("InvoiceFlow.Identity.Infrastructure");

    public static readonly Assembly Api =
        Assembly.Load("InvoiceFlow.Identity.Api");
}