using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record PaymentNumber
{
    public const int MaxLength = 50;

    private PaymentNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PaymentNumberRequired);

        var normalizedValue = value.Trim().ToUpperInvariant();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.PaymentNumberTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static PaymentNumber Create(string value) => new(value);

    public override string ToString() => Value;
}
