using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record PaymentTermDays
{
    public int Value { get; }

    private PaymentTermDays(int value)
    {
        if (value < 0)
            throw new DomainException(DomainErrors.PaymentTermDaysInvalid);

        Value = value;
    }

    public static PaymentTermDays Create(int value) => new(value);

    public override string ToString() => Value.ToString();
}