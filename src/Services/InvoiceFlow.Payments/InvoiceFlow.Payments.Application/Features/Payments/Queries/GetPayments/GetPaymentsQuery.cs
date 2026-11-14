using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record GetPaymentsQuery(
    Guid TenantId)
    : IRequest<Result<List<PaymentDto>>>;
