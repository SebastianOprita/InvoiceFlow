using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record CurrencyCode
{
    public const int MaxLength = 3;

    public string Value { get; }

    private CurrencyCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CurrencyCodeRequired);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != MaxLength || !normalized.All(char.IsLetter))
            throw new DomainException(DomainErrors.CurrencyCodeInvalid);

        Value = normalized;
    }

    public static CurrencyCode Create(string value) => new(value);

    public override string ToString() => Value;
}