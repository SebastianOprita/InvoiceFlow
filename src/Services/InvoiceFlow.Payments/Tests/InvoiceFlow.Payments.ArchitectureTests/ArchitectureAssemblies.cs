using System.Reflection;

namespace InvoiceFlow.Payments.ArchitectureTests;

internal static class ArchitectureAssemblies
{
    public static readonly Assembly Domain =
        Assembly.Load("InvoiceFlow.Payments.Domain");

    public static readonly Assembly Application =
        Assembly.Load("InvoiceFlow.Payments.Application");

    public static readonly Assembly Infrastructure =
        Assembly.Load("InvoiceFlow.Payments.Infrastructure");

    public static readonly Assembly Api =
        Assembly.Load("InvoiceFlow.Payments.Api");
}
