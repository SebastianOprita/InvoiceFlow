using System.Data;
using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Payments.Application;

public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    void ClearTrackedChanges();
}
