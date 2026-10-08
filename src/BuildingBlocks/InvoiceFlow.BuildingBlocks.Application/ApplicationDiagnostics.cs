using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Application;

internal static class ApplicationDiagnostics
{
    public const string ActivitySourceName =
        "InvoiceFlow.BuildingBlocks.Application";

    public static readonly ActivitySource Source =
        new(ActivitySourceName);
}
