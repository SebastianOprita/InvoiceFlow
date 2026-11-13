using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public sealed record PaymentNumberPrefix
{
    public string Value { get; }

    private PaymentNumberPrefix(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PrefixRequired);

        Value = value.Trim();
    }

    public static PaymentNumberPrefix Create(string value) => new(value);

    public override string ToString() => Value;
}
