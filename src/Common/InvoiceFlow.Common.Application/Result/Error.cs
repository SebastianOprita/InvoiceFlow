namespace InvoiceFlow.Common.Application;

public sealed class Error
{
    public ErrorType Type { get; }
    public string Code { get; }
    public string Message { get; }

    public Error(ErrorType type, string code, string message)
    {
        Type = type;
        Code = code;
        Message = message;
    }
}
