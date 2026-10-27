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
    : IRequestHandler<AddInvoiceLineCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(
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
            return Result<InvoiceDto>.Failure(hashResult);

        var requestHash = hashResult.Value;

        var existingIdempotencyRecord =
            await idempotencyRepository.GetIdempotencyRecordAsync(
                cmd.TenantId,
                IdempotencyOperation.AddInvoiceLine,
                key,
                cancellationToken);

        if (existingIdempotencyRecord is not null)
            return await ReplayAddInvoiceLineAsync(
                cmd.InvoiceId,
                existingIdempotencyRecord,
                requestHash,
                cancellationToken);

        var productResult =
            await productsService.GetProductAsync(
                cmd.TenantId,
                cmd.ProductCode,
                cancellationToken);

        if (productResult.IsFailure || productResult.Value is null)
            return Result<InvoiceDto>.Failure(productResult);

        var now = dateTimeProvider.GetUtcNow();
        var quantity = Quantity.Create(cmd.Quantity);
        var discountRate = Percentage.Create(cmd.DiscountRate ?? 0);

        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct =>
        {
            var transactionIdempotencyRecord =
                await idempotencyRepository.GetIdempotencyRecordAsync(
                    cmd.TenantId,
                    IdempotencyOperation.AddInvoiceLine,
                    key,
                    ct);

            if (transactionIdempotencyRecord is not null)
                return await ReplayAddInvoiceLineAsync(
                    cmd.InvoiceId,
                    transactionIdempotencyRecord,
                    requestHash,
                    ct);

            var invoice =
                await invoiceRepository.GetTrackedInvoiceByIdAsync(
                    cmd.TenantId,
                    cmd.InvoiceId,
                    ct);

            if (invoice is null)
                return Result<InvoiceDto>.Failure(ApplicationErrors.InvoiceNotFound);

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
                    IdempotencyOperation.AddInvoiceLine,
                    key,
                    cancellationToken);

            if (committedIdempotencyRecord is not null)
            {
                return await ReplayAddInvoiceLineAsync(
                    cmd.InvoiceId,
                    committedIdempotencyRecord,
                    requestHash,
                    cancellationToken);
            }
        }

        return result;
    }

    private async Task<Result<InvoiceDto>> ReplayAddInvoiceLineAsync(
        Guid invoiceId,
        IdempotencyRecord record,
        RequestHash requestHash,
        CancellationToken cancellationToken)
    {
        if (!record.MatchesRequest(requestHash))
            return Result<InvoiceDto>.Failure(ApplicationErrors.IdempotencyKeyReused);

        var invoice = await invoiceRepository.GetInvoiceByIdAsync(
            record.TenantId,
            invoiceId,
            cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(ApplicationErrors.IdempotencyResourceNotFound);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
