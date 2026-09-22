using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record CustomerName
{
    public const int MaxLength = 64;
    public string Value { get; }

    private CustomerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerNameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerNameTooLong);

        Value = normalized;
    }

    public static CustomerName Create(string value) => new(value);

    public override string ToString() => Value;
}
