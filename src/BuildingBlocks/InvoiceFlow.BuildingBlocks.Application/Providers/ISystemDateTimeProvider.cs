namespace InvoiceFlow.BuildingBlocks.Application;

public interface ISystemDateTimeProvider
{
    DateTimeOffset GetUtcNow();
}
