using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record PostalCode
{
    public const int MaxLength = 20;

    public string Value { get; }

    private PostalCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PostalCodeRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.PostalCodeTooLong);

        Value = normalized;
    }

    public static PostalCode Create(string value) => new(value);

    public override string ToString() => Value;
}