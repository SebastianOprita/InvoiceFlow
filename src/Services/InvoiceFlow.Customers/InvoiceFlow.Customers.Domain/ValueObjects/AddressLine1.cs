using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record AddressLine1
{
    public const int MaxLength = 100;

    public string Value { get; }

    private AddressLine1(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.AddressLine1Required);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.AddressLine1TooLong);

        Value = normalized;
    }

    public static AddressLine1 Create(string value) => new(value);

    public override string ToString() => Value;
}