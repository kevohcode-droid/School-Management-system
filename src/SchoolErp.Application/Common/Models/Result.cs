namespace SchoolErp.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; protected init; }
    public IReadOnlyList<string> Errors { get; protected init; } = Array.Empty<string>();

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { Succeeded = true, Value = value };
    public static new Result<T> Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
}
