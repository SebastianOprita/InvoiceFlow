using InvoiceFlow.BuildingBlocks.Domain;
using System.Globalization;

namespace InvoiceFlow.Products.Domain;

public sealed record Money
{
    private Money(decimal amount, CurrencyCode currencyCode)
    {
        if (currencyCode is null)
            throw new DomainException(DomainErrors.CurrencyCodeRequired);

        if (amount < 0)
            throw new DomainException(DomainErrors.MoneyInvalid);

        if (amount != decimal.Round(amount, currencyCode.DecimalPlaces, MidpointRounding.ToEven))
            throw new DomainException(DomainErrors.MoneyDecimalPlacesInvalid);

        Amount = amount;
        CurrencyCode = currencyCode;
    }

    public decimal Amount { get; }

    public CurrencyCode CurrencyCode { get; }

    public static Money Create(decimal amount, CurrencyCode currencyCode) => new(amount, currencyCode);

    public static Money Zero(CurrencyCode currencyCode) => new(0, currencyCode);

    public override string ToString()
    {
        var format = CurrencyCode.DecimalPlaces == 0
            ? "0"
            : $"0.{new string('0', CurrencyCode.DecimalPlaces)}";

        return $"{Amount.ToString(format, CultureInfo.InvariantCulture)} " + CurrencyCode.Value;
    }
}
