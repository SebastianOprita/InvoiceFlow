using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record CustomerCode
{
    public const int MaxLength = 36;

    public string Value { get; }
    
    private CustomerCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerCodeRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerCodeTooLong);

        Value = normalized;
    }

    public static CustomerCode Create(string value) => new(value);

    public override string ToString() => Value;
}
