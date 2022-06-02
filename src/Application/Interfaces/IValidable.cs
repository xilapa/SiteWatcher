namespace SiteWatcher.Application.Interfaces;

public interface IValidable
{
    Task<string[]> ValidateAsyncWith(dynamic validator);
}