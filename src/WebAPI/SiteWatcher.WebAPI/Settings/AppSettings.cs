namespace SiteWatcher.WebAPI.Settings;

public class AppSettings
{
    [ConfigurationKeyName("DATABASE_URL")]
    public string ConnectionString { get; set; } = null!;

    public string FrontEndUrl { get; set; } = null!;
    public byte[] RegisterKey { get; set; } = null!;
    public byte[] AuthKey { get; set; } = null!;

    [ConfigurationKeyName("Redis_ConnectionString")]
    public string RedisConnectionString { get; set; } = null!;

    public string CorsPolicy { get; set; } = null!;
    public byte[] InvalidToken { get; set; } = null!;
    public string ApiKeyName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
}
