using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IAppSettings AddSettings(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var appSettings = new AppSettings(env);
        configuration.Bind(appSettings);
        services.AddSingleton<IAppSettings>(_ => appSettings);
        services.AddSingleton<IGoogleSettings>(f => f.GetRequiredService<IConfiguration>().Get<GoogleSettings>());
        services.AddSingleton<IEmailSettings>(f => f.GetRequiredService<IConfiguration>().Get<EmailSettings>());
        return appSettings;
    }
}