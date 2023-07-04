using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Infra;
using HttpClient = SiteWatcher.Infra.Http.HttpClient;

namespace SiteWatcher.Worker.Jobs;

public static class JobConfigurator
{
    public static IServiceCollection SetupJobs(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        serviceCollection
            .AddScoped<IUserAlertsService, UserAlertsService>()
            .AddAuthService()
            .AddHttpClient()
            .AddScoped<IHttpClient, HttpClient>()
            .AddSingletonSession()
            .AddApplication();

        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection.AddHostedService<ExecuteAlertsPeriodically>();

        return serviceCollection;
    }
}