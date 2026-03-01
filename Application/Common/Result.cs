namespace Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null, 200);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? error, int statusCode)
        : base(isSuccess, error, statusCode)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, null, 200);
    public static new Result<T> Failure(string error, int statusCode = 400) => new(false, default, error, statusCode);
}
