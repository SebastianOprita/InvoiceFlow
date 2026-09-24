namespace InvoiceFlow.BuildingBlocks.Application;

public class Result
{
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;
    public ApplicationError? Error { get; }

    protected Result()
    { }

    protected Result(ApplicationError error)
    {
        Error = error;
    }

    protected Result(Result failedResult)
    {
        if (failedResult.IsSuccess || failedResult.Error is null)
        {
            throw new InvalidOperationException(
                "Cannot create a failure result from a successful result.");
        }

        Error = failedResult.Error;
    }

    public static Result Success() => new();

    public static Result Failure(ApplicationError error) => new(error);

    public static Result Failure(Result failedResult) => new(failedResult);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value)
    {
        Value = value;
    }

    private Result(ApplicationError error)
        : base(error)
    { }

    private Result(Result failedResult)
        : base(failedResult)
    { }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(ApplicationError error) => new(error);

    public static new Result<T> Failure(Result failedResult) => new(failedResult);
}
