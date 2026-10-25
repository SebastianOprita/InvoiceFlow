using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Products.Application;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
