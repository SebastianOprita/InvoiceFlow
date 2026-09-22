using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed record State
{
    public const int MaxLength = 50;

    public string Value { get; }

    private State(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.StateRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(DomainErrors.StateTooLong);

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