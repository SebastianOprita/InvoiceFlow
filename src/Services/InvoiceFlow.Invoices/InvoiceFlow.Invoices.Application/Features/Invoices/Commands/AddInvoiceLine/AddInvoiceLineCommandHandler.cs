using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal sealed class AddInvoiceLineCommandHandler(
    IInvoiceRepository invoiceRepository,
    IIdempotencyRepository idempotencyRepository,
    IProductsService productsService,
    IRequestHasher requestHasher,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<AddInvoiceLineCommand, Result<InvoiceLineDto>>
{
    public async Task<Result<InvoiceLineDto>> Handle(
        AddInvoiceLineCommand cmd,
        CancellationToken cancellationToken)
    {
        var key = IdempotencyKey.Create(cmd.IdempotencyKey);

        var hashResult = requestHasher.Hash(
            new
            {
                cmd.TenantId,
                cmd.InvoiceId,
                cmd.ProductCode,
                cmd.Quantity,
                cmd.DiscountRate
            });

        if (hashResult.IsFailure || hashResult.Value is null)
            return Result<InvoiceLineDto>.Failure(hashResult);

        var requestHash = hashResult.Value;

        var existing =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                cmd.TenantId,
                IdempotencyOperation.AddInvoiceLine,
                key,
                cancellationToken);

        if (existing is not null)
            return await ReplayAddInvoiceLineAsync(
                cmd.InvoiceId,
                existing,
                requestHash,
                cancellationToken);

        var productResult =
            await productsService.GetProductAsync(
                cmd.TenantId,
                cmd.ProductCode,
                cancellationToken);

        if (productResult.IsFailure || productResult.Value is null)
            return Result<InvoiceLineDto>.Failure(productResult);

        var now = dateTimeProvider.GetUtcNow();
        var quantity = Quantity.Create(cmd.Quantity);
        var discountRate = Percentage.Create(cmd.DiscountRate ?? 0);

        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct =>
        {
            var transactionExisting =
                await idempotencyRepository.GetIdempotencyRecordAsync(
                    cmd.TenantId,
                    IdempotencyOperation.AddInvoiceLine,
                    key,
                    ct);

            if (transactionExisting is not null)
                return await ReplayAddInvoiceLineAsync(
                    cmd.InvoiceId,
                    transactionExisting,
                    requestHash,
                    ct);

            var invoice =
                await invoiceRepository.GetTrackedInvoiceByIdAsync(
                    cmd.TenantId,
                    cmd.InvoiceId,
                    ct);

            if (invoice is null)
                return Result<InvoiceLineDto>.Failure(ApplicationErrors.InvoiceNotFound);

            var invoiceLine =
                invoice.AddProductLine(
                    Guid.CreateVersion7(),
                    productResult.Value,
                    quantity,
                    discountRate,
                    now);

            idempotencyRepository.Add(
                IdempotencyRecord.Create(
                    cmd.TenantId,
                    IdempotencyOperation.AddInvoiceLine,
                    key,
                    requestHash,
                    invoiceLine.Id,
                    now));

            var saveResult = await unitOfWork.SaveChangesAsync(ct);
            if (saveResult.IsFailure)
                return Result<InvoiceLineDto>.Failure(saveResult);

            return Result<InvoiceLineDto>.Success(invoiceLine.ToDto());
        },
        cancellationToken);

        if (result.IsFailure &&
            (result.Error == ApplicationErrors.IdempotencyKeyConflict ||
             result.Error == ApplicationErrors.ConcurrencyConflict))
        {
            unitOfWork.ClearTrackedChanges();

            var committedRecord =
                await idempotencyRepository.GetIdempotencyRecordAsync(
                    cmd.TenantId,
                    IdempotencyOperation.AddInvoiceLine,
                    key,
                    cancellationToken);

            if (committedRecord is not null)
            {
                return await ReplayAddInvoiceLineAsync(
                    cmd.InvoiceId,
                    committedRecord,
                    requestHash,
                    cancellationToken);
            }
        }

        return result;
    }

    private async Task<Result<InvoiceLineDto>> ReplayAddInvoiceLineAsync(
        Guid invoiceId,
        IdempotencyRecord record,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (!record.MatchesRequest(requestHash))
            return Result<InvoiceLineDto>.Failure(ApplicationErrors.IdempotencyKeyReused);

        var invoiceLine = await invoiceRepository.GetInvoiceLineByIdAsync(
            record.TenantId,
            invoiceId,
            record.ResourceId,
            cancellationToken);

        if (invoiceLine is null)
            return Result<InvoiceLineDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        return Result<InvoiceLineDto>.Success(invoiceLine.ToDto());
    }
}
