using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using MediatR;

namespace InvoiceFlow.Payments.Application;

internal sealed class CreatePaymentCommandHandler(
    IPaymentsRepository paymentsRepository,
    IIdempotencyRecordsRepository idempotencyRepository,
    ICustomersService customersService,
    IRequestHasher requestHasher,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(
        CreatePaymentCommand cmd,
        CancellationToken cancellationToken)
    {
        var key = IdempotencyKey.Create(cmd.IdempotencyKey);

        var hashResult = requestHasher.Hash(
            new
            {
                cmd.TenantId,
                cmd.CustomerCode,
                cmd.CurrencyCode,
                cmd.Amount,
                cmd.PaymentMethod,
                cmd.Reference,
                cmd.Notes
            });

        if (hashResult.IsFailure || hashResult.Value is null)
            return Result<PaymentDto>.Failure(hashResult);

        var requestHash = hashResult.Value;

        var existingResult = await TryReplayIdempotentRequestAsync(
            cmd.TenantId,
            key,
            requestHash,
            cancellationToken);
        if (existingResult is not null)
            return existingResult;

        var customerResult =
            await customersService.GetCustomerAsync(
                cmd.TenantId,
                cmd.CustomerCode,
                cancellationToken);

        if (customerResult.IsFailure || customerResult.Value is null)
            return Result<PaymentDto>.Failure(customerResult);

        var currencyCode = CurrencyCode.Create(cmd.CurrencyCode);
        var amount = Money.Create(cmd.Amount, currencyCode);
        var reference = PaymentReference.Create(cmd.Reference);
        var notes = PaymentNotes.CreateOptional(cmd.Notes);

        var now = dateTimeProvider.GetUtcNow();

        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct =>
        {
            var transactionReplay = await TryReplayIdempotentRequestAsync(
                cmd.TenantId,
                key,
                requestHash,
                ct);
            if (transactionReplay is not null)
                return transactionReplay;

            var payment = Payment.Create(
                tenantId: cmd.TenantId,
                id: Guid.CreateVersion7(),
                customerId: customerResult.Value.CustomerId,
                currencyCode: currencyCode,
                amount: amount,
                method: cmd.PaymentMethod,
                reference: reference,
                notes: notes,
                createdAtUtc: now);

            paymentsRepository.AddPayment(payment);

            idempotencyRepository.Add(
                IdempotencyRecord.Create(
                    cmd.TenantId,
                    IdempotencyOperation.CreatePayment,
                    key,
                    requestHash,
                    payment.Id,
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
            key,
            requestHash,
            cancellationToken);
    }

    private async Task<Result<PaymentDto>> RecoverFromConflictAsync(
        Result<PaymentDto> result,
        Guid tenantId,
        IdempotencyKey key,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (result.Error != ApplicationErrors.IdempotencyKeyConflict &&
            result.Error != ApplicationErrors.ConcurrencyConflict)
        {
            return result;
        }

        unitOfWork.ClearTrackedChanges();

        return await TryReplayIdempotentRequestAsync(
            tenantId,
            key,
            requestHash,
            cancellationToken) ?? result;
    }

    private async Task<Result<PaymentDto>?> TryReplayIdempotentRequestAsync(
        Guid tenantId,
        IdempotencyKey key,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        var idempotencyRecord =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                tenantId,
                IdempotencyOperation.CreatePayment,
                key,
                cancellationToken);

        return idempotencyRecord is null
            ? null
            : await ReplayCreatePaymentAsync(
                idempotencyRecord,
                requestHash,
                cancellationToken);
    }

    private async Task<Result<PaymentDto>> ReplayCreatePaymentAsync(
        IdempotencyRecord record,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (!record.MatchesRequest(requestHash))
            return Result<PaymentDto>.Failure(ApplicationErrors.IdempotencyKeyReused);

        var payment = await paymentsRepository.GetPaymentByIdAsync(
            record.TenantId,
            record.ResourceId,
            cancellationToken);

        if (payment is null)
            return Result<PaymentDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        return Result<PaymentDto>.Success(payment.ToDto());
    }
}
