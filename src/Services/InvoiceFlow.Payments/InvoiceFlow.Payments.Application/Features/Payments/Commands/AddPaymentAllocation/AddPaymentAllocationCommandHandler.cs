using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using MediatR;

namespace InvoiceFlow.Payments.Application;

internal sealed class AddPaymentAllocationCommandHandler(
    IPaymentsRepository paymentsRepository,
    IIdempotencyRecordsRepository idempotencyRepository,
    IRequestHasher requestHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AddPaymentAllocationCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(
       AddPaymentAllocationCommand cmd,
       CancellationToken cancellationToken)
    {
        var key = IdempotencyKey.Create(cmd.IdempotencyKey);

        var hashResult = requestHasher.Hash(
            new
            {
                cmd.TenantId,
                cmd.PaymentId,
                cmd.InvoiceId,
                cmd.Amount
            });

        if (hashResult.IsFailure || hashResult.Value is null)
            return Result<PaymentDto>.Failure(hashResult);

        var requestHash = hashResult.Value;

        var replay = await TryReplayIdempotentRequestAsync(
            cmd.TenantId,
            cmd.PaymentId,
            key,
            requestHash,
            cancellationToken);
        if (replay is not null)
            return replay;

        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct =>
        {
            var transactionReplay = await TryReplayIdempotentRequestAsync(
                cmd.TenantId,
                cmd.PaymentId,
                key,
                requestHash,
                ct);
            if (transactionReplay is not null)
                return transactionReplay;

            var payment = await paymentsRepository.GetPaymentWithLockAsync(
                cmd.TenantId,
                cmd.PaymentId,
                ct);

            if (payment is null)
                return Result<PaymentDto>.Failure(ApplicationErrors.PaymentNotFound);

            if (payment.Status != PaymentLifecycleStatus.Posted)
                return Result<PaymentDto>.Failure(ApplicationErrors.PaymentMustBePosted);

            var balance = await paymentsRepository.GetInvoicePaymentBalanceWithLockAsync(
                cmd.TenantId,
                cmd.InvoiceId,
                ct);

            if (balance is null)
                return Result<PaymentDto>.Failure(ApplicationErrors.InvoicePaymentBalanceNotFound);

            if (balance.CurrencyCode != payment.CurrencyCode)
                return Result<PaymentDto>.Failure(ApplicationErrors.PaymentCurrencyMismatch);

            if (cmd.Amount != decimal.Round(
                cmd.Amount,
                payment.CurrencyCode.DecimalPlaces,
                MidpointRounding.ToEven))
                return Result<PaymentDto>.Failure(ApplicationErrors.AllocationAmountPrecisionInvalid);

            var amount = Money.Create(cmd.Amount, payment.CurrencyCode);

            if (amount.Amount > balance.OutstandingAmount.Amount)
                return Result<PaymentDto>.Failure(ApplicationErrors.AllocationAmountExceedsInvoiceOutstanding);

            if (amount.CurrencyCode != payment.CurrencyCode)
                return Result<PaymentDto>.Failure(ApplicationErrors.PaymentCurrencyMismatch);

            if (amount.Amount > payment.UnallocatedAmount.Amount)
                return Result<PaymentDto>.Failure(ApplicationErrors.InsufficientUnallocatedPaymentAmount);

            var now = timeProvider.GetUtcNow();

            var paymentAllocation = payment.AddAllocation(
                Guid.CreateVersion7(),
                balance,
                amount,
                cmd.AllocatedBy,
                now);

            idempotencyRepository.Add(
                IdempotencyRecord.Create(
                    cmd.TenantId,
                    IdempotencyOperation.AddPaymentAllocation,
                    key,
                    requestHash,
                    paymentAllocation.Id,
                    now));

            var saveResult = await unitOfWork.SaveChangesAsync(ct);
            if (saveResult.IsFailure)
                return Result<PaymentDto>.Failure(saveResult);

            return Result<PaymentDto>.Success(payment.ToDto());
        },
        cancellationToken);

        return await RecoverFromConflictAsync(
            result,
            cmd.TenantId,
            cmd.PaymentId,
            key,
            requestHash,
            cancellationToken);
    }

    private async Task<Result<PaymentDto>> RecoverFromConflictAsync(
        Result<PaymentDto> result,
        Guid tenantId,
        Guid paymentId,
        IdempotencyKey key,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (result.Error != ApplicationErrors.IdempotencyKeyConflict &&
            result.Error != ApplicationErrors.ConcurrencyConflict)
            return result;

        unitOfWork.ClearTrackedChanges();

        return await TryReplayIdempotentRequestAsync(
            tenantId,
            paymentId,
            key,
            requestHash,
            cancellationToken) ?? result;
    }

    private async Task<Result<PaymentDto>?> TryReplayIdempotentRequestAsync(
        Guid tenantId,
        Guid paymentId,
        IdempotencyKey key,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        var idempotencyRecord =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                tenantId,
                IdempotencyOperation.AddPaymentAllocation,
                key,
                cancellationToken);

        return idempotencyRecord is null
            ? null
            : await ReplayAddPaymentAllocationAsync(
                paymentId,
                idempotencyRecord,
                requestHash,
                cancellationToken);
    }

    private async Task<Result<PaymentDto>> ReplayAddPaymentAllocationAsync(
        Guid paymentId,
        IdempotencyRecord record,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (!record.MatchesRequest(requestHash))
            return Result<PaymentDto>.Failure(ApplicationErrors.IdempotencyKeyReused);

        var payment = await paymentsRepository.GetPaymentByIdWithAllocationsAsync(
            record.TenantId,
            paymentId,
            cancellationToken);

        if (payment is null)
            return Result<PaymentDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        if (!payment.Allocations.Any(allocation => allocation.Id == record.ResourceId))
            return Result<PaymentDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        return Result<PaymentDto>.Success(payment.ToDto());
    }
}
