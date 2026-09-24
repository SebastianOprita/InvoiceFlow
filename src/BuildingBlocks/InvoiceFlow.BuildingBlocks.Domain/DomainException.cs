namespace InvoiceFlow.BuildingBlocks.Domain;

public sealed class DomainException : Exception
{
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public DomainException(string errorCode, string errorMessage)
        : base(errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public DomainException((string errorCode, string errorMessage) error)
        : base(error.errorMessage)
    {
        ErrorCode = error.errorCode;
        ErrorMessage = error.errorMessage;
    }

    public DomainException(DomainError error)
        : base(error.ErrorMessage)
    {
        ErrorCode = error.ErrorCode;
        ErrorMessage = error.ErrorMessage;
    }
}
