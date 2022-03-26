namespace SiteWatcher.WebAPI.Settings;

public class AppSettings
{
    [ConfigurationKeyName("DATABASE_URL")]
    public string ConnectionString { get; set;  }
    public string FrontEndUrl { get; set; }
    public byte[] RegisterKey { get; set; }
    public byte[] AuthKey { get; set; }

    [ConfigurationKeyName("Redis_ConnectionString")]
    public string RedisConnectionString { get; set; }

    public string CorsPolicy { get; set; }
    public byte[] InvalidToken { get; set;}

    public string ApiKeyName { get; set; }
    public string ApiKey { get; set; }
}
