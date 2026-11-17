using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Domain;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

internal class GetInvoicesQueryHandler(IInvoicesRepository invoiceRepository)
    : IRequestHandler<GetInvoicesQuery, Result<List<InvoiceDto>>>
{
    public async Task<Result<List<InvoiceDto>>> Handle(GetInvoicesQuery qry, CancellationToken cancellationToken)
    {
        var invoices = await invoiceRepository.GetAllInvoicesWithLinesAsync(qry.TenantId, cancellationToken);
        if (invoices.Count == 0)
            return Result<List<InvoiceDto>>.Success([]);

        var issuedInvoices = invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Issued)
            .ToArray();
        var paymentSummaries = issuedInvoices.Length == 0
            ? new List<InvoicePaymentSummary>()
            : await invoiceRepository.GetPaymentSummariesAsync(
                qry.TenantId,
                issuedInvoices.Select(invoice => invoice.Id).ToArray(),
                cancellationToken);
        var paymentSummariesByInvoiceId = paymentSummaries
            .GroupBy(x => x.InvoiceId)
            .ToDictionary(x => x.Key, x => x.First());

        var invoiceDtos = invoices
            .Select(invoice => invoice.ToDto(
                paymentSummariesByInvoiceId.GetValueOrDefault(invoice.Id)))
            .ToList();

        return Result<List<InvoiceDto>>.Success(invoiceDtos);
    }
}
