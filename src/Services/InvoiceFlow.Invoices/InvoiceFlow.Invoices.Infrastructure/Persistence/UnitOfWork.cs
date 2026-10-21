using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly InvoicesDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        InvoicesDbContext dbContext,
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

            return Result.Failure(ApplicationErrors.ConcurrencyConflict);
        }
        catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("SaveChanges was cancelled.");
            throw;
        }
    }

    public async Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await operation(cancellationToken);

        if (result.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);

            return result;
        }

        await transaction.CommitAsync(cancellationToken);

        return result;
    }
}
