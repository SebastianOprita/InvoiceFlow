using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal class GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery qry, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetInvoiceByIdAsync(qry.TenantId, qry.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result<InvoiceDto>.Failure(ApplicationErrors.InvoiceNotFound);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
