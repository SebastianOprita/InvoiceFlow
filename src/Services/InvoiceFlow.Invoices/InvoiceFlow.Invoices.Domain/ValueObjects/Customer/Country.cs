using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record Country
{
    public const int MaxLength = 56;

    public string Value { get; }

    private Country(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerCountryRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerCountryTooLong);

        Value = normalized;
    }

    public static Country Create(string value) => new(value);

    public override string ToString() => Value;
}
