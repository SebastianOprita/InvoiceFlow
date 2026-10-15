using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record ProductName
{
    public const int MaxLength = 100;

    private ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ProductNameRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.ProductNameTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static ProductName Create(string value) => new(value);

    public override string ToString() => Value;
}
