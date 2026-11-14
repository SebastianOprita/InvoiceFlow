using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Domain;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record CreatePaymentCommand(
    string IdempotencyKey,
    Guid TenantId,
    string CustomerCode,
    string CurrencyCode,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string Reference,
    string Notes
) :IRequest<Result<PaymentDto>>;
