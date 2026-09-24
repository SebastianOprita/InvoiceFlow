using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record RegistrationNumber
{
    public const int MaxLength = 15;

    public string Value { get; }

    private RegistrationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.RegistrationNumberRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.RegistrationNumberTooLong);

        Value = normalized;
    }

    public static RegistrationNumber Create(string value) => new(value);

    public override string ToString() => Value;
}