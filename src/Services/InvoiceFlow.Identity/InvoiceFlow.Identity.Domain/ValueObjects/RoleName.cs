using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record RoleName
{
    public const int MaxLength = 32;
    public string Value { get; }

    private RoleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.NameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.NameTooLong);

        Value = normalized;
    }

    public static RoleName Create(string value) => new(value);

    public override string ToString() => Value;
}
