using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record IdempotencyKey
{
    public const int MaxLength = 128;

    public string Value { get; }

    private IdempotencyKey(string value)
    {
        Value = value;
    }

    public static IdempotencyKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.IdempotencyKeyRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.IdempotencyKeyTooLong);

        return new IdempotencyKey(normalizedValue);
    }

    public override string ToString() => Value;
}
