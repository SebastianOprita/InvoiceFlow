namespace InvoiceFlow.BuildingBlocks.Application;

public interface ISystemDateTimeProvider
{
    DateTime Now { get; }
}
