using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record ServiceClientSecretName
{
    public const int MaxLength = 100;

    public string Value { get; }

    private ServiceClientSecretName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ServiceClientSecretNameRequired);

        if (value.Trim().Length > MaxLength)
            throw new DomainException(DomainErrors.ServiceClientSecretNameInvalid);

        Value = value.Trim();
    }

    public static ServiceClientSecretName Create(string value) => new(value);

    public override string ToString() => Value;
}
