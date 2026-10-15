using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record State
{
    public const int MaxLength = 50;

    public string Value { get; }

    private State(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.CustomerStateRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.CustomerStateTooLong);

        Value = normalized;
    }

    public static State Create(string value) => new(value);

    public static State? CreateOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new State(value);
    }

    public override string ToString() => Value;
}
