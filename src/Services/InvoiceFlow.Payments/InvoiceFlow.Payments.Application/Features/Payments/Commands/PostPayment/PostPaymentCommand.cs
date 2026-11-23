using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record PostPaymentCommand(
    Guid TenantId,
    Guid PaymentId,
    Guid PostedBy)
    : IRequest<Result>;
