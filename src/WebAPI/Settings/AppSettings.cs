using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Settings;

public class AppSettings : IAppSettings
{
    public AppSettings(IWebHostEnvironment env)
    {
        IsDevelopment = env.IsDevelopment();
    }

    public bool IsDevelopment { get; }
    public string ConnectionString { get; set; }
    public string FrontEndUrl { get; set; } = null!;
    public byte[] RegisterKey { get; set; } = null!;
    public byte[] AuthKey { get; set; } = null!;

    [ConfigurationKeyName("Redis_ConnectionString")]
    public string RedisConnectionString { get; set; } = null!;

    public string CorsPolicy { get; set; } = null!;
    public byte[] InvalidToken { get; set; } = null!;
    public string ApiKeyName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string IdHasherSalt { get; set; } = null!;
    public int MinimumHashedIdLength { get; set; }

    string IAppSettings.MessageIdKey => MessageIdKey;

    public bool InMemoryStorageAndQueue { get; set; }
    public string EmailNotificationRoutingKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public const string MessageIdKey = "message-id";
}
