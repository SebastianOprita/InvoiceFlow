using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record ServiceClientSecretDescription
{
    public const int MaxLength = 500;

    public string Value { get; }

    private ServiceClientSecretDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ServiceClientSecretDescriptionRequired);

        if (value.Trim().Length > MaxLength)
            throw new DomainException(DomainErrors.ServiceClientSecretDescriptionInvalid);

        Value = value.Trim();
    }

    public static ServiceClientSecretDescription Create(string value) =>
        new(value);

    public static ServiceClientSecretDescription? CreateOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : new(value);

    public override string ToString() => Value;
}
