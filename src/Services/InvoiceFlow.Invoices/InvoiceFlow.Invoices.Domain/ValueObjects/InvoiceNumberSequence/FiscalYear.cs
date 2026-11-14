using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record FiscalYear
{
    public int Value { get; }

    private FiscalYear(int value)
    {
        if (value < 2000 || value > 2100)
            throw new DomainException(DomainErrors.YearInvalid);

        Value = value;
    }

    public static FiscalYear Create(int value) => new(value);

    public override string ToString() => Value.ToString();
}
