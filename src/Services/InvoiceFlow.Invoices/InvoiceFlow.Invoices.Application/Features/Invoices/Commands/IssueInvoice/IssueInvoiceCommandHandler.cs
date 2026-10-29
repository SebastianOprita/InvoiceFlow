using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal sealed class IssueInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IInvoiceNumberRepository invoiceNumberRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<IssueInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        IssueInvoiceCommand cmd,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.ExecuteInTransactionAsync(
        async ct => 
        {
            var invoice = await invoiceRepository.GetInvoiceWithLockAsync(
                cmd.TenantId,
                cmd.InvoiceId,
                ct);

            if (invoice is null)
                return Result.Failure(ApplicationErrors.InvoiceNotFound);
                
            if (invoice.Status == InvoiceStatus.Issued)
                return Result.Success();

            if (invoice.Status != InvoiceStatus.Draft)
                return Result.Failure(ApplicationErrors.InvoiceCannotBeIssued);

            var now = dateTimeProvider.GetUtcNow();
            var issueDate = DateOnly.FromDateTime(now.UtcDateTime);

            var invoiceNumber =
                await invoiceNumberRepository.AllocateNextWithLockAsync(
                    cmd.TenantId,
                    now.UtcDateTime.Year,
                    ct);

            invoice.Issue(InvoiceNumber.Create(invoiceNumber), issueDate, now);

            return await unitOfWork.SaveChangesAsync(ct);

        },
        cancellationToken);

        if (result.IsFailure && result.Error == ApplicationErrors.ConcurrencyConflict)
        {
            unitOfWork.ClearTrackedChanges();

            var currentInvoice = await invoiceRepository.GetInvoiceByIdAsync(
                cmd.TenantId,
                cmd.InvoiceId,
                cancellationToken);

            if (currentInvoice?.Status == InvoiceStatus.Issued)
                return Result.Success();
        }

        return result;
    }
}
