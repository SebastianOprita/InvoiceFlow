using System.Globalization;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

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
