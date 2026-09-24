using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record UserEmail
{
    public const int MaxLength = 64;
    public string Value { get; }

    private UserEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.EmailRequired);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.EmailTooLong);

        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
            throw new DomainException(DomainErrors.EmailInvalid);

        Value = normalized;
    }

    public static UserEmail Create(string value) => new(value);

    public override string ToString() => Value;
}
