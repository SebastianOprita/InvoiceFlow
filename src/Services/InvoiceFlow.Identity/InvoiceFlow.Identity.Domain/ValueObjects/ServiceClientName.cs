using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record ServiceClientName
{
    public const int MaxLength = 150;

    public string Value { get; }

    private ServiceClientName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ServiceClientNameRequired);

        if (value.Trim().Length > MaxLength)
            throw new DomainException(DomainErrors.ServiceClientNameInvalid);

        Value = value.Trim();
    }

    public static ServiceClientName Create(string value) => new(value);

    public override string ToString() => Value;
}
