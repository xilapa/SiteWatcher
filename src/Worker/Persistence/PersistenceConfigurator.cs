using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Persistence;

public static class PersistenceConfigurator
{
    public static IServiceCollection SetupPersistence(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        serviceCollection
            .AddScoped<SiteWatcherContext>(_ => new SiteWatcherContext(settings.AppSettings, null!));
        return serviceCollection;
    }

}