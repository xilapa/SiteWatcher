namespace SiteWatcher.Application.Common.Commands;

public interface ICommandResult<T>
{
    IEnumerable<string> Errors { get; }
    bool Success { get; }
    T? Value { get; }
}