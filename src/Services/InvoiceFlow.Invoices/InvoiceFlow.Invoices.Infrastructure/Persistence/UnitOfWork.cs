using System.Data;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
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

    public Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        => ExecuteWithRetryAsync(
            operation,
            result => result.IsFailure,
            () => Result.Failure(ApplicationErrors.TransientFailure),
            cancellationToken,
            isolationLevel);

    public Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        => ExecuteWithRetryAsync(
            operation,
            result => result.IsFailure,
            () => Result<T>.Failure(ApplicationErrors.TransientFailure),
            cancellationToken,
            isolationLevel);

    public void ClearTrackedChanges()
    {
        _dbContext.ChangeTracker.Clear();
    }

    private async Task<TResult> ExecuteWithRetryAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        Func<TResult, bool> isFailure,
        Func<TResult> exhaustedFailure,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        var attempt = 0;

        try
        {
            return await executionStrategy.ExecuteAsync(async ct =>
            {
                attempt++;
                if (attempt > 1)
                {
                    _logger.LogWarning(
                        "Retrying invoice database transaction (attempt {Attempt}).",
                        attempt);
                    _dbContext.ChangeTracker.Clear();
                }

                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(isolationLevel, ct);

                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SET LOCK_TIMEOUT 5000;",
                    ct);

                var result = await operation(ct);
                if (isFailure(result))
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    return result;
                }

                await transaction.CommitAsync(ct);
                return result;
            }, cancellationToken);
        }
        catch (RetryLimitExceededException ex)
        {
            _logger.LogError(
                ex,
                "Invoice database transaction failed after exhausting transient-failure retries.");

            return exhaustedFailure();
        }
    }

    private static bool IsIdempotencyKeyConflict(
        DbUpdateException exception)
    {
        var sqlException = exception.InnerException as SqlException;

        return sqlException is not null
            && sqlException.Number is 2601 or 2627
            && sqlException.Message.Contains(
                IdempotencyIndexName,
                StringComparison.OrdinalIgnoreCase);
    }
}
