using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record IssueInvoiceCommand(
    Guid TenantId,
    Guid InvoiceId)
    : IRequest<Result>;
