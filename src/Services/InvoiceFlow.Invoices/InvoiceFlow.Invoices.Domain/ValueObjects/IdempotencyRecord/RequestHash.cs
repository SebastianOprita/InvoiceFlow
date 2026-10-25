using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record RequestHash
{
    public const int Sha256HexLength = 64;

    public string Value { get; }

    private RequestHash(string value)
    {
        Value = value;
    }

    public static RequestHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.RequestHashRequired);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length != Sha256HexLength || !normalizedValue.All(Uri.IsHexDigit))
            throw new DomainException(DomainErrors.RequestHashInvalid);

        return new RequestHash(normalizedValue.ToUpperInvariant());
    }

    public override string ToString() => Value;
}
