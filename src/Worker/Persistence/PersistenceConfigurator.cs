using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Domain.Emails.Repositories;
using SiteWatcher.Domain.Users.Repositories;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Persistence.Repositories;
using SiteWatcher.Infra.Repositories;

namespace SiteWatcher.Worker.Persistence;

public static class PersistenceConfigurator
{
    public static IServiceCollection SetupPersistence(this IServiceCollection serviceCollection, WorkerSettings settings, IHostEnvironment env)
    {
        // IsDevelopment is used by the SiteWatcher Context to log or not the queries on console
        var appSettings = new WorkerAppSettings { IsDevelopment = env.IsDevelopment(), ConnectionString = settings.DbConnectionString};
        serviceCollection
            .AddScoped<SiteWatcherContext>(_ => new SiteWatcherContext(appSettings, mediator: null))
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IEmailRepository, EmailRepository>();
        return serviceCollection;
    }

}