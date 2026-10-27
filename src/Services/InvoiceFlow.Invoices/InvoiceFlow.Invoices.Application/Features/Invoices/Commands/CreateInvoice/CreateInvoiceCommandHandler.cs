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

        var existingIdempotencyRecord =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                cmd.TenantId,
                IdempotencyOperation.CreateInvoice,
                key,
                cancellationToken);

        if (existingIdempotencyRecord is not null)
            return await ReplayCreateInvoiceAsync(
                existingIdempotencyRecord,
                requestHash,
                cancellationToken);

        var customerResult =
            await customersService.GetCustomerAsync(
                cmd.TenantId,
                cmd.CustomerCode,
                cancellationToken);

        if (customerResult.IsFailure || customerResult.Value is null)
            return Result<InvoiceDto>.Failure(customerResult);

        var now = dateTimeProvider.GetUtcNow();

        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct =>
        {
            var transactionIdempotencyRecord =
                await idempotencyRepository.GetIdempotencyRecordAsync(
                    tenantId: cmd.TenantId,
                    operation: IdempotencyOperation.CreateInvoice,
                    key: key,
                    cancellationToken: ct);

            if (transactionIdempotencyRecord is not null)
                return await ReplayCreateInvoiceAsync(
                    transactionIdempotencyRecord,
                    requestHash,
                    ct);

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

        if (result.IsFailure &&
            (result.Error == ApplicationErrors.IdempotencyKeyConflict ||
             result.Error == ApplicationErrors.ConcurrencyConflict))
        {
            unitOfWork.ClearTrackedChanges();

            var committedIdempotencyRecord =
                await idempotencyRepository.GetIdempotencyRecordAsync(
                    cmd.TenantId,
                    IdempotencyOperation.CreateInvoice,
                    key,
                    cancellationToken);

            if (committedIdempotencyRecord is not null)
            {
                return await ReplayCreateInvoiceAsync(
                    committedIdempotencyRecord,
                    requestHash,
                    cancellationToken);
            }
        }

        return result;
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
