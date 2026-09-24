using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record LastName
{
    public const int MaxLength = 32;
    public string Value { get; }

    private LastName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.LastNameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.LastNameTooLong);

        Value = normalized;
    }

    public static LastName Create(string value) => new(value);

    public override string ToString() => Value;
}
