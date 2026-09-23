namespace InvoiceFlow.Common.Application;

public interface ISystemDateTimeProvider
{
    DateTime Now { get; }
}
