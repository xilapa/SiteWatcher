using Application.Interfaces;

namespace SiteWatcher.WebAPI;

public interface IStartup
{
    IAppSettings AppSettings { get; }
    void ConfigureServices(IServiceCollection services, IWebHostEnvironment env);
    void Configure(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory);
}
