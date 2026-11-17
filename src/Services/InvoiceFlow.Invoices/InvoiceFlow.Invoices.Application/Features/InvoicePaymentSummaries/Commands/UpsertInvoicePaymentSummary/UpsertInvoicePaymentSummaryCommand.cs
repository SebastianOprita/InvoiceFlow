using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Invoices.Application;

public sealed record UpsertInvoicePaymentSummaryCommand(
    Guid TenantId,
    Guid InvoiceId,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string CurrencyCode,
    long PaymentBalanceVersion,
    DateTimeOffset SourceUpdatedAtUtc,
    DateTimeOffset AppliedAtUtc)
    : IRequest<Result>;
