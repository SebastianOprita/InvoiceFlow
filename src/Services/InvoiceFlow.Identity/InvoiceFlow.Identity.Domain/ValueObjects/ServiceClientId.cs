using System.Text.RegularExpressions;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Identity.Domain;

public sealed partial record ServiceClientId
{
    public const int MaxLength = 100;

    public string Value { get; }

    private ServiceClientId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.ServiceClientIdRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength || !ClientIdRegex().IsMatch(normalizedValue))
            throw new DomainException(DomainErrors.ServiceClientIdInvalid);

        Value = normalizedValue;
    }

    public static ServiceClientId Create(string value) => new(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-z0-9]+(?:[-_][a-z0-9]+)*$")]
    private static partial Regex ClientIdRegex();
}
