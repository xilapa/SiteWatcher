namespace SiteWatcher.Application.Interfaces;

public interface IDapperQueries
{
    public string GetUserByGoogleId { get; }
    public string DeleteActiveUserById { get; }
}