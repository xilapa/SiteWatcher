using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Domain.Emails.Repositories;
using SiteWatcher.Domain.Users.Repositories;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Persistence.Repositories;
using SiteWatcher.Infra.Repositories;

namespace SiteWatcher.Worker.Persistence;

public static class PersistenceConfigurator
{
    public static IServiceCollection SetupPersistence(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddDataContext<SiteWatcherContext>(addMigrator: false)
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IEmailRepository, EmailRepository>();
        return serviceCollection;
    }
}