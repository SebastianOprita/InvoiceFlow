using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record ServiceClientSecretHash
{
    public const int MaxLength = 64;

    public string Value { get; }

    private ServiceClientSecretHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ServiceClientSecretHashRequired);

        if (value.Length != MaxLength || !value.All(Uri.IsHexDigit))
            throw new DomainException(DomainErrors.ServiceClientSecretHashInvalid);

        Value = value.ToUpperInvariant();
    }

    public static ServiceClientSecretHash Create(string value) =>
        new(value);

    public override string ToString() => Value;
}
