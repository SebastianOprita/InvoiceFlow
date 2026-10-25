using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record CreateInvoiceCommand(
    string IdempotencyKey,
    Guid TenantId,
    string CustomerCode,
    string Notes
) : IRequest<Result<InvoiceDto>>;
