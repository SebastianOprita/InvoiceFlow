using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Application;

public interface IUnitOfWork : IDisposable
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
