using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Interfaces;

public interface IStartup
{
    AppSettings AppSettings { get; }
    void ConfigureServices(IServiceCollection services, IWebHostEnvironment env);
    void Configure(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory);
}
