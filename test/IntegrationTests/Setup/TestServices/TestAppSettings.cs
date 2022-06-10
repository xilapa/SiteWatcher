using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestAppSettings : IAppSettings
{
    public TestAppSettings()
    {
        RegisterKey = new byte[128];
        AuthKey = new byte[128];
        InvalidToken = new byte[128];
        foreach (var i in Enumerable.Range(0, 128))
        {
            RegisterKey[i] = (byte)i;
            AuthKey[i] = (byte)(i + 1);
            InvalidToken[i] = (byte)(i + 2);
        }

        CorsPolicy = "TestPolicy";
        ApiKeyName = "TestKey";
        ApiKey = "TestValue";
        FrontEndUrl = "https://xilapa.com";
        IsDevelopment = true;
        ConnectionString = "TestConnectionString";
        RedisConnectionString = "RedisConnectionString";
    }

    public bool IsDevelopment { get; }
    public string ConnectionString { get; set; }
    public string FrontEndUrl { get; set; }
    public byte[] RegisterKey { get; set; }
    public byte[] AuthKey { get; set; }
    public string RedisConnectionString { get; set; }
    public string CorsPolicy { get; set; }
    public byte[] InvalidToken { get; set; }
    public string ApiKeyName { get; set; }
    public string ApiKey { get; set; }
}