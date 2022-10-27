using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Persistence;

public static class PersistenceConfigurator
{
    public static IServiceCollection SetupPersistence(this IServiceCollection serviceCollection, WorkerSettings settings, IHostEnvironment env)
    {
        // This settings is used by the SiteWatcher Context to log or not the queries on console
        settings.AppSettings.IsDevelopment = env.IsDevelopment();
        serviceCollection
            .AddScoped<SiteWatcherContext>(_ => new SiteWatcherContext(settings.AppSettings, mediator: null));
        return serviceCollection;
    }

}