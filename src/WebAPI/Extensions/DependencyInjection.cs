using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services.AddSingleton<IAppSettings>(f => f.GetRequiredService<IConfiguration>().Get<AppSettings>());
        services.AddSingleton<IGoogleSettings>(f => f.GetRequiredService<IConfiguration>().Get<GoogleSettings>());
        return services;
    }
}