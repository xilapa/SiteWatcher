using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Persistence;

public static class PersistenceConfigurator
{
    public static IServiceCollection SetupPersistence(this IServiceCollection serviceCollection, WorkerSettings settings, IHostEnvironment env)
    {
        // IsDevelopment is used by the SiteWatcher Context to log or not the queries on console
        var appSettings = new WorkerAppSettings { IsDevelopment = env.IsDevelopment(), ConnectionString = settings.DbConnectionString};
        serviceCollection
            .AddScoped<SiteWatcherContext>(_ => new SiteWatcherContext(appSettings, mediator: null));
        return serviceCollection;
    }

}