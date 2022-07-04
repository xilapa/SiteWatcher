namespace SiteWatcher.WebAPI.Filters.Cache;

public class CacheInfo
{
    public string Key { get; set; } = null!;
    public string HashFieldName { get; set; } = null!;
    public TimeSpan Expiration { get; set; }
}