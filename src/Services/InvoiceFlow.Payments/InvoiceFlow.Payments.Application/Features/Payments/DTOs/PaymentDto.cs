using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public sealed record PaymentDto(
    Guid TenantId,
    Guid Id,
    string? Number,
    Guid CustomerId,
    string CurrencyCode,
    decimal Amount,
    decimal AllocatedAmount,
    decimal UnallocatedAmount,
    PaymentMethod Method,
    string? Reference,
    DateOnly? PaymentDate,
    PaymentLifecycleStatus Status,
    string? Notes,
    DateTimeOffset? PostedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyCollection<PaymentAllocationDto> Allocations);
