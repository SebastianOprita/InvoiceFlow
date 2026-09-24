namespace InvoiceFlow.BuildingBlocks.Currency;

public static class Iso4217
{
    private static readonly Dictionary<string, CurrencyDefinition> _currencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["EUR"] = new("EUR", 978, 2),
            ["USD"] = new("USD", 840, 2),
            ["GBP"] = new("GBP", 826, 2),
            ["RON"] = new("RON", 946, 2),
            ["CHF"] = new("CHF", 756, 2),
            ["PLN"] = new("PLN", 985, 2),
            ["CZK"] = new("CZK", 203, 2),
            ["HUF"] = new("HUF", 348, 2),
            ["JPY"] = new("JPY", 392, 0),
            ["KWD"] = new("KWD", 414, 3),
            ["BHD"] = new("BHD", 48, 3),
            ["OMR"] = new("OMR", 512, 3),
        };

    public static bool TryGet(
        string code,
        out CurrencyDefinition currency) =>
        _currencies.TryGetValue(code, out currency);

    public static CurrencyDefinition Get(string code)
    {
        if (!_currencies.TryGetValue(code, out var currency))
            throw new KeyNotFoundException($"Unknown currency '{code}'.");

        return currency;
    }

    public static IReadOnlyCollection<CurrencyDefinition> All =>
        _currencies.Values;
}
