using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record CustomerEmail
{
    public const int MaxLength = 64;

    public string Value { get; }

    private CustomerEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.EmailRequired);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.EmailTooLong);

        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
            throw new DomainException(DomainErrors.EmailInvalid);

        Value = normalized;
    }

    public static CustomerEmail Create(string value) => new(value);

    public static CustomerEmail? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new CustomerEmail(value);
    }

    public override string ToString() => Value;
}