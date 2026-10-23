namespace InvoiceFlow.BuildingBlocks.Application;

public class SystemDateTimeProvider : ISystemDateTimeProvider
{
#pragma warning disable RS0030 // This adapter is the approved system clock access point.
    public DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
#pragma warning restore RS0030
}
