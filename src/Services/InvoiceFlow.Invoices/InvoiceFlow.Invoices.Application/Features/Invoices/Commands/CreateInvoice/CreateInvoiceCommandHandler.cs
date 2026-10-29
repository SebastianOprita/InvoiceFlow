using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal sealed class CreateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IIdempotencyRepository idempotencyRepository,
    ICustomersService customersService,
    IRequestHasher requestHasher,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(
        CreateInvoiceCommand cmd,
        CancellationToken cancellationToken)
    {
        var key = IdempotencyKey.Create(cmd.IdempotencyKey);

        var hashResult = requestHasher.Hash(
            new {
                cmd.TenantId,
                cmd.CustomerCode,
                cmd.Notes
            });

        if (hashResult.IsFailure || hashResult.Value is null)
            return Result<InvoiceDto>.Failure(hashResult);

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
            return Result<InvoiceDto>.Failure(customerResult);

        if (!customerResult.Value.IsActive)
            return Result<InvoiceDto>.Failure(ApplicationErrors.CustomerInactive);

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

            var invoice = Invoice.Create(
                tenantId: cmd.TenantId,
                id: Guid.CreateVersion7(),
                customer: customerResult.Value,
                notes: InvoiceNotes.CreateOptional(cmd.Notes),
                createdAtUtc: now);

            invoiceRepository.AddInvoice(invoice);

            idempotencyRepository.Add(
                IdempotencyRecord.Create(
                    cmd.TenantId,
                    IdempotencyOperation.CreateInvoice,
                    key,
                    requestHash,
                    invoice.Id,
                    now));

            var saveResult = await unitOfWork.SaveChangesAsync(ct);
            if (saveResult.IsFailure)
                return Result<InvoiceDto>.Failure(saveResult);

            return Result<InvoiceDto>.Success(invoice.ToDto());
        },
        cancellationToken);

        return await RecoverFromConflictAsync(
            result,
            cmd.TenantId,
            key,
            requestHash,
            cancellationToken);
    }

    private async Task<Result<InvoiceDto>> RecoverFromConflictAsync(
        Result<InvoiceDto> result,
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

    private async Task<Result<InvoiceDto>?> TryReplayIdempotentRequestAsync(
        Guid tenantId,
        IdempotencyKey key,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        var idempotencyRecord =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                tenantId,
                IdempotencyOperation.CreateInvoice,
                key,
                cancellationToken);

        return idempotencyRecord is null
            ? null
            : await ReplayCreateInvoiceAsync(
                idempotencyRecord,
                requestHash,
                cancellationToken);
    }

    private async Task<Result<InvoiceDto>> ReplayCreateInvoiceAsync(
        IdempotencyRecord record,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (!record.MatchesRequest(requestHash))
            return Result<InvoiceDto>.Failure(ApplicationErrors.IdempotencyKeyReused);

        var invoice = await invoiceRepository.GetInvoiceByIdAsync(
            record.TenantId,
            record.ResourceId,
            cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
