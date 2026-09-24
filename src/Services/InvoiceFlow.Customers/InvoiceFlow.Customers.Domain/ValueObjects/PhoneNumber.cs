using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record PhoneNumber
{
    public const int MaxLength = 15;

    public string Value { get; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PhoneNumberRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.PhoneNumberTooLong);

        Value = normalized;
    }

    public static PhoneNumber Create(string value) => new(value);

    public static PhoneNumber? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new PhoneNumber(value);
    }

    public override string ToString() => Value;
}