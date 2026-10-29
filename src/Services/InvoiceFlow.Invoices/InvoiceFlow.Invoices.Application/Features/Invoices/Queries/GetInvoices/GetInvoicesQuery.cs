using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record GetInvoicesQuery(
    Guid TenantId)
    : IRequest<Result<List<InvoiceDto>>>;
