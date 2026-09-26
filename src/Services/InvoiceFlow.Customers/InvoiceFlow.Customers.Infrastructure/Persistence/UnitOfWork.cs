using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.Customers.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CustomersDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        CustomersDbContext dbContext,
        ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while saving changes.");

            return Result.Failure(ApplicationErrors.ConcurencyConflict);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while saving changes.");

            return Result.Failure(ApplicationErrors.DbSaveFailed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
