using InvoiceFlow.BuildingBlocks.Domain;
using System.Globalization;

namespace InvoiceFlow.Invoices.Domain;

public sealed record Quantity
{
    private Quantity(decimal value)
    {
        if (value <= 0)
            throw new DomainException(DomainErrors.QuantityInvalid);

        Value = value;
    }

    public decimal Value { get; }

    public static Quantity Create(decimal value) => new(value);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
