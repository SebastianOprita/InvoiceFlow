using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record TenantName
{
    public const int MaxLength = 64;

    public string Value { get; }

    private TenantName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.NameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.NameTooLong);

        Value = normalized;
    }

    public static TenantName Create(string value) => new(value);

    public override string ToString() => Value;
}
