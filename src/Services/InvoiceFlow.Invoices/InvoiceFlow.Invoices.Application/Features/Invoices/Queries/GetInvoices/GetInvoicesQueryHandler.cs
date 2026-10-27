using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal class GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    : IRequestHandler<GetInvoicesQuery, Result<List<InvoiceDto>>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(GetInvoicesQuery qry, CancellationToken cancellationToken)
    {
        var invoices = await invoiceRepository.GetAllInvoicesWithLinesAsync(qry.TenantId, cancellationToken);
        var invoiceDtos = invoices.Select(x => x.ToDto()).ToList();

        return Result<List<InvoiceDto>>.Success(invoiceDtos);
    }
}
