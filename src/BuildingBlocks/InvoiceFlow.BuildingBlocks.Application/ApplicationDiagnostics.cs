using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Application;

internal class ApplicationDiagnostics
{
    public const string ActivitySourceName =
        "InvoiceFlow.BuildingBlocks.Application";

    public static readonly ActivitySource Source =
        new(ActivitySourceName);

}
