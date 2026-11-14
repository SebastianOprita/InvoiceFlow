namespace InvoiceFlow.Payments.Application;

public sealed record PaymentAllocationDto(
    Guid TenantId,
    Guid Id,
    Guid PaymentId,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    string CurrencyCode,
    DateTimeOffset CreatedAtUtc);
