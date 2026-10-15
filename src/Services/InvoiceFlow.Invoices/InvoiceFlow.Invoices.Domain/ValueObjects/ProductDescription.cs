using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record ProductDescription
{
    public const int MaxLength = 500;

    private ProductDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ProductDescriptionRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.ProductDescriptionTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static ProductDescription Create(string value) => new(value);

    public static ProductDescription? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new ProductDescription(value);
    }

    public override string ToString() => Value;
}
