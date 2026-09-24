using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class RefreshTokenHash
{
    public const int MaxLength = 512;

    public string Value { get; }

    private RefreshTokenHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.TokenHashRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.TokenHashTooLong);

        Value = normalized;
    }

    public static RefreshTokenHash Create(string value) => new(value);

    public override string ToString() => Value;
}
