using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record City
{
    public const int MaxLength = 50;

    public string Value { get; }

    private City(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerCityRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerCityTooLong);

        Value = normalized;
    }

    public static City Create(string value) => new(value);

    public override string ToString() => Value;
}
