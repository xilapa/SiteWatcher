using System.Text.RegularExpressions;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Settings;

public class AppSettings : IAppSettings
{
    private readonly IWebHostEnvironment _env;

    public AppSettings(IWebHostEnvironment env)
    {
        _env = env;
        IsDevelopment = env.IsDevelopment();
    }

    public bool IsDevelopment { get; }
    private string _connectionString = null!;

    [ConfigurationKeyName("DATABASE_URL")]
    public string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = _env.IsDevelopment() ? value : ParseHerokuConnectionString(value);
    }
    public string FrontEndUrl { get; set; } = null!;
    public byte[] RegisterKey { get; set; } = null!;
    public byte[] AuthKey { get; set; } = null!;

    [ConfigurationKeyName("Redis_ConnectionString")]
    public string RedisConnectionString { get; set; } = null!;

    public string CorsPolicy { get; set; } = null!;
    public byte[] InvalidToken { get; set; } = null!;
    public string ApiKeyName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;

    private static string ParseHerokuConnectionString(string connectionString)
    {
        var regexString = new Regex(@"\:\/\/(?<user>.*?)\:(?<password>.*?)\@(?<host>.*?)\:(?<port>.*?)\/(?<database>.*)");
        var matches = regexString.Match(connectionString).Groups;
        return  $"Server={matches["host"].Value};" +
                $"Port={matches["port"].Value};" +
                $"Database={matches["database"].Value};" +
                $"User Id={matches["user"].Value};" +
                $"Password={matches["password"].Value}";
    }
}
