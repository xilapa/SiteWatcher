namespace SiteWatcher.Application.Common.Commands;

public abstract class CommandResult
{
    public static ValueResult<T> FromValue<T>(T value) => new (value);
    public static ErrorResult FromError(string value) => new (value);
    public static ErrorResult FromErrors(IEnumerable<string> value) => new (value);

    public static readonly CommandResult Empty = new EmptyResult();
}

public sealed class ValueResult<T> : CommandResult
{
    public ValueResult(T value)
    {
        Value = value;
    }

    public T Value { get; init; }
}

public sealed class EmptyResult : CommandResult;

public sealed class ErrorResult: CommandResult
{
    public ErrorResult(string error)
    {
        Errors = new [] {error};
    }

    public ErrorResult(IEnumerable<string> errors)
    {
        Errors = errors;
    }

    public IEnumerable<string> Errors { get; }
}