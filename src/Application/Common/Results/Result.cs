using SiteWatcher.Domain.Common.Errors;

namespace SiteWatcher.Application.Common.Results;

public interface IResult
{
    Error? Error { get; }
    void SetError(Error error);
}

public sealed class Result : IResult
{
    public Result()
    {
    }

    public Result(Error error)
    {
        Error = error;
    }

    public Error? Error { get; private set; }

    public void SetError(Error error)
    {
        Error = error;
    }

    public static Result Empty { get; } = new();

    public static implicit operator Result(Error error) => new(error);
}

public sealed class Result<T> : IResult where T : class
{
    public Result()
    {
    }

    public Result(T value)
    {
        Value = value;
    }

    public Result(Error error)
    {
        Error = error;
    }

    public Error? Error { get; private set; }
    public T? Value { get; }

    public void SetError(Error error)
    {
        Error = error;
    }

    public static Result<T> Empty { get; } = new();

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}