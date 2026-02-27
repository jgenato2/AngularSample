namespace Server.Application.Models;

public sealed class OperationResult<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorType? ErrorType { get; }

    private OperationResult(bool success, T? value, string? error, ErrorType? errorType)
    {
        Success = success;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static OperationResult<T> Ok(T value) => new(true, value, null, null);

    public static OperationResult<T> Fail(string error, ErrorType errorType) =>
        new(false, default, error, errorType);
}
