namespace SiteWatcher.Application.Interfaces;

public interface IValidable
{
    bool IsValid { get; }
    bool IsInvalid { get; }
    IEnumerable<string> Errors { get; }
    IEnumerable<string> Validate();
}