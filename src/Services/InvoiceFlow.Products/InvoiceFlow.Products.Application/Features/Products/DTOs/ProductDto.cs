using InvoiceFlow.Products.Domain;

namespace InvoiceFlow.Products.Application;

public sealed record ProductDto(
    Guid TenantId,
    Guid Id,
    string ProductCode,
    string ProductName,
    string? ProductDescription,
    UnitOfMeasure UnitOfMeasure,
    MoneyDto UnitPrice,
    decimal TaxRate,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
