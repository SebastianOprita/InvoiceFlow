using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record GetPaymentByIdQuery(
    Guid TenantId,
    Guid PaymentId)
    : IRequest<Result<PaymentDto>>;
