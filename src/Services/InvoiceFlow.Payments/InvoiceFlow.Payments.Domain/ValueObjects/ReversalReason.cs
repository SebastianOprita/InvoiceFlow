using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record ReversalReason
{
    public const int MaxLength = 1000;

    private ReversalReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ReversalReasonRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.ReversalReasonTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static ReversalReason Create(string value) => new(value);

    public override string ToString() => Value;
}
