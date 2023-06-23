using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestAppSettings : IAppSettings
{
    public const string TestHashIdSalt = "SuperSecretHashSalt";
    public const int TestHashedIdLength = 3;

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
        FrontEndAuthUrl = "https://xilapa.com/auth";
        IsDevelopment = true;
        ConnectionString = "TestConnectionString";
        RedisConnectionString = "RedisConnectionString";
        IdHasherSalt = TestHashIdSalt;
        MinimumHashedIdLength = TestHashedIdLength;
        MessageIdKey = "message-id-testing";
        InMemoryStorageAndQueue = true;
        DisableDataProtectionRedisStore = true;
    }

    public bool IsDevelopment { get; }
    public string ConnectionString { get; set; }
    public string FrontEndUrl { get; set; }
    public string FrontEndAuthUrl { get; set; }
    public byte[] RegisterKey { get; set; }
    public byte[] AuthKey { get; set; }
    public string RedisConnectionString { get; set; }
    public string CorsPolicy { get; set; }
    public byte[] InvalidToken { get; set; }
    public string ApiKeyName { get; set; }
    public string ApiKey { get; set; }
    public string IdHasherSalt { get; set; }
    public int MinimumHashedIdLength { get; set; }
    public string MessageIdKey { get; set; }
    public bool InMemoryStorageAndQueue  { get; set; }
    public bool DisableDataProtectionRedisStore { get; set; }
}