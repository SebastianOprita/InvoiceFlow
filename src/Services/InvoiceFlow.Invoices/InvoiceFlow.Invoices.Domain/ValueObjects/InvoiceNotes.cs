using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceNotes
{
    public const int MaxLength = 2000;

    private InvoiceNotes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.InvoiceNotesRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.InvoiceNotesTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static InvoiceNotes Create(string value) => new(value);
    public static InvoiceNotes? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new InvoiceNotes(value);
    }

    public override string ToString() => Value;
}
