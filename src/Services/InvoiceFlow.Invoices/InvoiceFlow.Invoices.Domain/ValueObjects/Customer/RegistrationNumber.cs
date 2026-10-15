using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record RegistrationNumber
{
    public const int MaxLength = 15;

    public string Value { get; }

    private RegistrationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerRegistrationNumberRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerRegistrationNumberTooLong);

        Value = normalized;
    }

    public static RegistrationNumber Create(string value) => new(value);

    public override string ToString() => Value;
}
