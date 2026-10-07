namespace InvoiceFlow.BuildingBlocks.Domain;

public sealed class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException(string errorCode, string errorMessage)
        : base($"{errorCode}: {errorMessage}")
    {
        Error = new DomainError(errorCode, errorMessage);
    }

    public DomainException((string errorCode, string errorMessage) error)
        : base($"{error.errorCode}: {error.errorMessage}")
    {
        Error = new DomainError(error.errorCode, error.errorMessage);
    }

    public DomainException(DomainError error)
        : base($"{error.ErrorCode}: {error.ErrorMessage}")
    {
        Error = error;
    }
}
