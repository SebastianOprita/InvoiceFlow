using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record FirstName
{
    public const int MaxLength = 32;
    public string Value { get; }

    private FirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.FirstNameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.FirstNameTooLong);

        Value = normalized;
    }

    public static FirstName Create(string value) => new(value);

    public override string ToString() => Value;
}
