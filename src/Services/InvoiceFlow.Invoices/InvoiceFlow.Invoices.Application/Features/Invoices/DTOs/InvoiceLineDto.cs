using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public sealed record InvoiceLineDto(
    Guid Id,
    int Position,
    Guid? ProductId,
    string? ProductCode,
    string Description,
    decimal Quantity,
    UnitOfMeasure UnitOfMeasure,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal NetAmount,
    decimal GrossAmount);
