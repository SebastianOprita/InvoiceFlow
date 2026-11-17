using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal class GetInvoiceByIdQueryHandler(IInvoicesRepository invoiceRepository)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery qry, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetInvoiceByIdWithLinesAsync(qry.TenantId, qry.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(ApplicationErrors.InvoiceNotFound);

        var paymentSummary = invoice.Status == InvoiceStatus.Issued
            ? await invoiceRepository.GetPaymentSummaryAsync(
                qry.TenantId,
                qry.InvoiceId,
                cancellationToken)
            : null;

        return Result<InvoiceDto>.Success(invoice.ToDto(paymentSummary));
    }
}
