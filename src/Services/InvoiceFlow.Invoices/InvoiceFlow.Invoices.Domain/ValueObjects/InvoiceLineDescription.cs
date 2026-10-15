using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceLineDescription
{
    public const int MaxLength = 500;

    private InvoiceLineDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.InvoiceLineDescriptionRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.InvoiceLineDescriptionTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static InvoiceLineDescription Create(string value) => new(value);

    public override string ToString() => Value;
}
