namespace InvoiceFlow.Common.Application;

public interface IUnitOfWork : IDisposable
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
