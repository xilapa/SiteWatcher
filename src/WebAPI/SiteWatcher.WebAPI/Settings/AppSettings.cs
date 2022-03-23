namespace SiteWatcher.WebAPI.Settings;

public class AppSettings
{
    [ConfigurationKeyName("Database_ConnectionString")]
    public string ConnectionString { get; set;  }
    public string FrontEndUrl { get; set; }
    public byte[] RegisterKey { get; set; }
    public byte[] AuthKey { get; set; }

    [ConfigurationKeyName("Redis_ConnectionString")]
    public string RedisConnectionString { get; set; }

    public string CorsPolicy { get; set; }
    public byte[] InvalidToken {get; set;}
}
