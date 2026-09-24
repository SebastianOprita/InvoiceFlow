namespace InvoiceFlow.BuildingBlocks.Application;

public sealed class ApplicationError
{
    public ApplicationErrorType Type { get; }
    public string Code { get; }
    public string Message { get; }

    public ApplicationError(ApplicationErrorType type, string code, string message)
    {
        Type = type;
        Code = code;
        Message = message;
    }
}
