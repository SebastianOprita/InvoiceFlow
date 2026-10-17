using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record PaymentReference
{
    public const int MaxLength = 100;

    private PaymentReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PaymentReferenceRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.PaymentReferenceTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static PaymentReference Create(string value) => new(value);
    public static PaymentReference? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new PaymentReference(value);
    }

    public override string ToString() => Value;
}
