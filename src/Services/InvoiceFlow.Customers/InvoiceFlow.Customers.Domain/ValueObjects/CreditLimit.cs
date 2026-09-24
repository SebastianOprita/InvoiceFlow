using InvoiceFlow.BuildingBlocks.Domain;
using System.Globalization;

namespace InvoiceFlow.Customers.Domain;

public sealed record CreditLimit
{
    public decimal Value { get; }

    private CreditLimit(decimal value)
    {
        if (value < 0)
            throw new DomainException(DomainErrors.CreditLimitInvalid);

        Value = value;
    }

    public static CreditLimit Create(decimal value) => new(value);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}