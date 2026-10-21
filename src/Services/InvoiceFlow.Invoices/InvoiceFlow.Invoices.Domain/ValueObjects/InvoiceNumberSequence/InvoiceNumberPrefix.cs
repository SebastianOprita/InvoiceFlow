using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceNumberPrefix
{
    public string Value { get; }

    private InvoiceNumberPrefix(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PrefixRequired);

        Value = value.Trim();
    }

    public static InvoiceNumberPrefix Create(string value) => new(value);

    public override string ToString() => Value;
}
