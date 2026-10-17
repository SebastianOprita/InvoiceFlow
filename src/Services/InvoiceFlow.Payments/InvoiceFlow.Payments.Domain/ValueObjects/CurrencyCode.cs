using InvoiceFlow.BuildingBlocks.Currency;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record CurrencyCode
{
    public const int MaxLength = 3;

    public string Value { get; }

    public CurrencyDefinition Definition => Iso4217.Get(Value);

    public byte DecimalPlaces => Definition.DecimalPlaces;

    public short NumericCode => Definition.NumericCode;

    private CurrencyCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CurrencyCodeRequired);

        var normalized = value.Trim().ToUpperInvariant();

        if (!Iso4217.TryGet(normalized, out _))
            throw new DomainException(DomainErrors.CurrencyCodeInvalid);

        Value = normalized;
    }

    public static CurrencyCode Create(string value) => new(value);

    public override string ToString() => Value;
}
