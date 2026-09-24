using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record RoleDescription
{
    public const int MaxLength = 128;
    public string Value { get; }

    private RoleDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.DescriptionRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.DescriptionTooLong);

        Value = normalized;
    }

    public static RoleDescription Create(string value) => new(value);

    public static RoleDescription? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new RoleDescription(value);
    }

    public override string ToString() => Value;
}
