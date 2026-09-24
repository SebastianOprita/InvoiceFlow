using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed class DeviceInfo
{
    public const int MaxLength = 256;

    public string Value { get; }

    private DeviceInfo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.DeviceInfoRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.DeviceInfoTooLong);

        Value = normalized;
    }

    public static DeviceInfo Create(string value) => new(value);

    public static DeviceInfo? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new DeviceInfo(value);
    }

    public override string ToString() => Value;
}
