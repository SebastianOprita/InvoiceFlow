using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record AddPaymentAllocationCommand(
    string IdempotencyKey,
    Guid TenantId,
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    Guid AllocatedBy) : IRequest<Result<PaymentDto>>;
