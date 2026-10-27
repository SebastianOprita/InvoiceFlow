using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record GetInvoiceByIdQuery(
    Guid TenantId,
    Guid InvoiceId)
    : IRequest<Result<InvoiceDto>>;
