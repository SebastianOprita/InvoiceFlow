namespace InvoiceFlow.BuildingBlocks.Currency;

public sealed record CurrencyDefinition(
    string Code,
    short NumericCode,
    byte DecimalPlaces);
