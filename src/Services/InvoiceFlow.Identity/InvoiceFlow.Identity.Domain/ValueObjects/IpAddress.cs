using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class IpAddress
{
    public const int MaxLength = 64;

    public string Value { get; }

    private IpAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.IpAddressRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.IpAddressTooLong);

        Value = normalized;
    }

    public static IpAddress Create(string value) => new(value);

    public static IpAddress? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new IpAddress(value);
    }

    public override string ToString() => Value;
}
