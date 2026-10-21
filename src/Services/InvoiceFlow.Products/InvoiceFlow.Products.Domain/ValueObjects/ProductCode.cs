using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Products.Domain;

public sealed record ProductCode
{
    public const int MaxLength = 50;

    private ProductCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ProductCodeRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
            throw new DomainException(DomainErrors.ProductCodeTooLong);

        Value = normalizedValue;
    }

    public string Value { get; }

    public static ProductCode Create(string value) => new(value);

    public static ProductCode? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new ProductCode(value);
    }

    public override string ToString() => Value;
}
