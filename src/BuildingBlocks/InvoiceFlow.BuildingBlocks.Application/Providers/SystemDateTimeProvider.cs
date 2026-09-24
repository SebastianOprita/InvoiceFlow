namespace InvoiceFlow.BuildingBlocks.Application;

public class SystemDateTimeProvider : ISystemDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}
