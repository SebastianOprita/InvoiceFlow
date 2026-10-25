using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.Invoices.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private const string IdempotencyIndexName = "UX_IdempotencyRecords_TenantId_Operation_Key";

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
        catch (DbUpdateException ex)
            when (IsIdempotencyKeyConflict(ex))
        {
            _logger.LogWarning(
                ex,
                "Duplicate idempotency key detected while saving changes.");

            return Result.Failure(
                ApplicationErrors.IdempotencyKeyConflict);

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

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
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

    private static bool IsIdempotencyKeyConflict(
        DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && sqlException.Number is 2601 or 2627
            && sqlException.Message.Contains(
                IdempotencyIndexName,
                StringComparison.OrdinalIgnoreCase);
    }
}
