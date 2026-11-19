using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Payments.Application;

public sealed record CreateInvoicePaymentBalanceCommand(
    Guid TenantId,
    Guid InvoiceId,
    Guid CustomerId,
    string InvoiceNumber,
    string CurrencyCode,
    decimal TotalAmount,
    DateTimeOffset CreatedAtUtc)
    : IRequest<Result>;
