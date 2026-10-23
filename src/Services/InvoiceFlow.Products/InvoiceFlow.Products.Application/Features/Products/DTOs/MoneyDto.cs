namespace InvoiceFlow.Products.Application;

public sealed record MoneyDto(
    decimal Amount,
    string CurrencyCode,
    byte DecimalPlaces);
