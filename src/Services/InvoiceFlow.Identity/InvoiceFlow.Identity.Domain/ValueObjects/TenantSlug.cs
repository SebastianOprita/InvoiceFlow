using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record TenantSlug
{
    public const int MaxLength = 64;

    public string Value { get; }

    private TenantSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.SlugRequired);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.SlugTooLong);

        if (normalized.StartsWith('-') || normalized.EndsWith('-'))
            throw new DomainException(DomainErrors.SlugStartsOrEndsInvalid);

        foreach (var ch in normalized)
        {
            var isLetterOrDigit = char.IsLetterOrDigit(ch);

            if (!isLetterOrDigit && ch != '-')
                throw new DomainException(DomainErrors.SlugInvalid);
        }

        Value = normalized;
    }

    public static TenantSlug Create(string value) => new(value);

    public override string ToString() => Value;
}
