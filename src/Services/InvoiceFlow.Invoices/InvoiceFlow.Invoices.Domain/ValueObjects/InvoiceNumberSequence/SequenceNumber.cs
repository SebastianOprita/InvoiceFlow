using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record SequenceNumber
{
    public long Value { get; }

    private SequenceNumber(long value)
    {
        if (value < 1)
            throw new DomainException(DomainErrors.SequenceNumberInvalid);

        Value = value;
    }

    public static SequenceNumber Create(long value) => new(value);

    public SequenceNumber Next()
    {
        if (Value == long.MaxValue)
            throw new DomainException(DomainErrors.SequenceNumberExceededMaxValue);

        return new SequenceNumber(Value + 1);
    }

    public override string ToString() => Value.ToString("D6");
}
