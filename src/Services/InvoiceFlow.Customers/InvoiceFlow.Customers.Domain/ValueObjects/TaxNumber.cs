using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record TaxNumber
{
    public const int MaxLength = 64;

    public string Value { get; }

    private TaxNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.TaxNumberRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.TaxNumberTooLong);

        Value = normalized;
    }

    public static TaxNumber Create(string value) => new(value);

    public static TaxNumber? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new TaxNumber(value);
    }

    public override string ToString() => Value;
}