using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record Percentage
{
    private Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new DomainException(DomainErrors.PercentageInvalid);

        Value = value;
    }

    public decimal Value { get; }

    public static Percentage Create(decimal value) => new(value);

    public static Percentage Zero => new(0);

    public override string ToString() => $"{Value}%";
}
