using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record AddInvoiceLineCommand(
    string IdempotencyKey,
    Guid TenantId,
    Guid InvoiceId,
    string ProductCode,
    decimal Quantity,
    decimal? DiscountRate)
    : IRequest<Result<InvoiceLineDto>>;
