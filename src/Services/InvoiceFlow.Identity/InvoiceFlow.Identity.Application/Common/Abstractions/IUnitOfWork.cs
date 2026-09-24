using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Identity.Application;

public interface IUnitOfWork : IDisposable
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
