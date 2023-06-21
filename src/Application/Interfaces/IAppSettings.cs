namespace SiteWatcher.Application.Interfaces;

public interface IAppSettings
{
    public bool IsDevelopment { get; }
    string ConnectionString { get; set; }
    string FrontEndUrl { get; set; }
    string FrontEndAuthUrl { get; set; }
    byte[] RegisterKey { get; set; }
    byte[] AuthKey { get; set; }
    string RedisConnectionString { get; set; }
    string CorsPolicy { get; set; }
    byte[] InvalidToken { get; set; }
    string ApiKeyName { get; set; }
    string ApiKey { get; set; }
    string IdHasherSalt { get; set; }
    int MinimumHashedIdLength { get; set; }
    string MessageIdKey { get; }
    public bool InMemoryStorageAndQueue { get; set; }
    public bool DisableDataProtectionRedisStore { get; set; }
}