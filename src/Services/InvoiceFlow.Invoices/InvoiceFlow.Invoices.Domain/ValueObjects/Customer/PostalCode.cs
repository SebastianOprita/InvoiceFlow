using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record PostalCode
{
    public const int MaxLength = 20;

    public string Value { get; }

    private PostalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerPostalCodeRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerPostalCodeTooLong);

        Value = normalized;
    }

    public static PostalCode Create(string value) => new(value);

    public override string ToString() => Value;
}
