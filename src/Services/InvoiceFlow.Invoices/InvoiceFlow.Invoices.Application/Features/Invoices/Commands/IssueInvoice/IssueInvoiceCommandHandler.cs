using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed class IssueInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IInvoiceNumberRepository invoiceNumberRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<IssueInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        IssueInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(
            async ct => 
            {
                var invoice = await invoiceRepository.GetForIssuingAsync(
                    request.TenantId,
                    request.InvoiceId,
                    ct);

                if (invoice is null)
                    return Result.Failure(ApplicationErrors.InvoiceNotFound);

                if (invoice.Status == InvoiceStatus.Issued)
                    return Result.Success();

                if (invoice.Status != InvoiceStatus.Draft)
                    return Result.Failure(ApplicationErrors.InvoiceCannotBeIssued);

                var now = dateTimeProvider.Now;

                var invoiceNumber =
                    await invoiceNumberRepository.AllocateNextAsync(
                        request.TenantId,
                        now.Year,
                        ct);

                invoice.Issue(InvoiceNumber.Create(invoiceNumber), now);

                return await unitOfWork.SaveChangesAsync(ct);

            },
            cancellationToken);
    }
}
