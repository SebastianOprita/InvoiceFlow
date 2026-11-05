using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record OrganizationName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private OrganizationName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.OrganizationNameRequired);

        if (value.Trim().Length > MaxLength)
            throw new DomainException(DomainErrors.OrganizationNameInvalid);

        Value = value.Trim();
    }

    public static OrganizationName Create(string value) => new(value);

    public override string ToString() => Value;
}
