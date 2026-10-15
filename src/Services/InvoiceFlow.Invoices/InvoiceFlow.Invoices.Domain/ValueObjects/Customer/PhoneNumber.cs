using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record PhoneNumber
{
    public const int MaxLength = 15;

    public string Value { get; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerPhoneRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerPhoneTooLong);

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
