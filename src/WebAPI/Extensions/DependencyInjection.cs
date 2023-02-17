using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.GoogleAuth;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IAppSettings AddSettings(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var appSettings = new AppSettings(env);
        configuration.Bind(appSettings);
        services.AddSingleton<IAppSettings>(_ => appSettings);
        // services.Configure<GoogleSettings>(configuration); // TODO: remove
        services.AddSingleton<IEmailSettings>(f => f.GetRequiredService<IConfiguration>().Get<EmailSettings>());
        return appSettings;
    }
}