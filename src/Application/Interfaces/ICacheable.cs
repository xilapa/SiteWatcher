namespace SiteWatcher.Application.Interfaces;

public interface ICacheable
{
    public TimeSpan Expiration { get; }
    public string HashFieldName { get; }
    string GetKey(ISession session);
}