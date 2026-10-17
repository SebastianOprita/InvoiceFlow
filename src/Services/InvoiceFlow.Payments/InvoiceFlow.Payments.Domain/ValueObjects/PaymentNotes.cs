using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record PaymentNotes
{
    public const int MaxLength = 2000;

    private PaymentNotes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PaymentNotesRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.PaymentNotesTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static PaymentNotes Create(string value) => new(value);
    public static PaymentNotes? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new PaymentNotes(value);
    }

    public override string ToString() => Value;
}
