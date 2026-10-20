using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Invoices.Application;

public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken = default);
}
