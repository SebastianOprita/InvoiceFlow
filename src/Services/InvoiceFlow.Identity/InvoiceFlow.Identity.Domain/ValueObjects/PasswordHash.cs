using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed record PasswordHash
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.PasswordHashRequired);

        Value = value;
    }

    public static PasswordHash Create(string value) => new(value);

    public override string ToString() => Value;
}
