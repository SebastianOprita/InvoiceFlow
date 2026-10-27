using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceNumber
{
    public const int MaxLength = 50;

    private InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.InvoiceNumberRequired);

        var normalizedValue = value.Trim().ToUpperInvariant();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.InvoiceNumberTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static InvoiceNumber Create(string value) => new(value);

    public static InvoiceNumber? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new InvoiceNumber(value);
    }

    public override string ToString() => Value;
}
