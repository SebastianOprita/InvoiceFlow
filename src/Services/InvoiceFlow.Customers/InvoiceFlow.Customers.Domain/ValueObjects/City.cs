using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record City
{
    public const int MaxLength = 50;

    public string Value { get; }

    private City(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CityRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CityTooLong);

        Value = normalized;
    }

    public static City Create(string value) => new(value);

    public override string ToString() => Value;
}