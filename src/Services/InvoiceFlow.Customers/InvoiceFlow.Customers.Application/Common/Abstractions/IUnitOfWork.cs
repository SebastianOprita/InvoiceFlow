using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Application;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
