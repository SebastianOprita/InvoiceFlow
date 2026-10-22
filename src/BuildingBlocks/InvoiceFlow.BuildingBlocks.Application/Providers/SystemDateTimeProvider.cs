namespace InvoiceFlow.BuildingBlocks.Application;

public class SystemDateTimeProvider : ISystemDateTimeProvider
{
    public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
}
