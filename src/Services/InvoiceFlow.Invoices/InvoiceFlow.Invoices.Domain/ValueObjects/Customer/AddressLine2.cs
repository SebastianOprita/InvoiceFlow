using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record AddressLine2
{
    public const int MaxLength = 100;

    public string Value { get; }

    private AddressLine2(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerAddressLine2Required);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerAddressLine2TooLong);

        Value = normalized;
    }

    public static AddressLine2 Create(string value) => new(value);

    public static AddressLine2? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new AddressLine2(value);
    }

    public override string ToString() => Value;
}
