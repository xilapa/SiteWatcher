namespace SiteWatcher.Application.Common.Commands;

public class CommandResult<T> : ICommandResult<T>
{
    public CommandResult() =>
        _errors = new List<string>();

    public CommandResult(T result) : this() =>
        Value = result;

    public CommandResult<T> WithError(string error)
    {
        _errors.Add(error);
        return this;
    }

    public CommandResult<T> WithValue(T value)
    {
        Value = value;
        return this;
    }

    public void SetError(string error)
    {
        _errors.Add(error);
    }

    private readonly List<string> _errors;
    public IEnumerable<string> Errors => _errors.ToArray();
    public bool Success => _errors.Count == 0;
    public T? Value { get; private set; }
}