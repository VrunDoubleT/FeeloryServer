namespace FeeloryBackend.Commons;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? error) : base(isSuccess, error)
        => Data = data;

    public static Result<T> Ok(T data) => new(true, data, null);
    public new static Result<T> Fail(string error) => new(false, default, error);
}