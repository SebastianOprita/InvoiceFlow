namespace InvoiceFlow.Common.Application;

public class SystemDateTimeProvider : ISystemDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}
