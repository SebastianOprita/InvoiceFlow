using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceYear
{
    public int Value { get; }

    private InvoiceYear(int value)
    {
        if (value < 2000 || value > 2100)
            throw new DomainException(DomainErrors.YearInvalid);

        Value = value;
    }

    public static InvoiceYear Create(int value) => new(value);

    public override string ToString() => Value.ToString();
}
