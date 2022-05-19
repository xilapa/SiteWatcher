namespace SiteWatcher.Application.Interfaces;

public interface IAppSettings
{
    public bool IsDevelopment { get; }
    string ConnectionString { get; set; }
    string FrontEndUrl { get; set; }
    byte[] RegisterKey { get; set; }
    byte[] AuthKey { get; set; }
    string RedisConnectionString { get; set; }
    string CorsPolicy { get; set; }
    byte[] InvalidToken { get; set; }
    string ApiKeyName { get; set; }
    string ApiKey { get; set; }
}