using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.DomainServices;
using HttpClient = SiteWatcher.Infra.Http.HttpClient;

namespace SiteWatcher.Worker.Jobs;

public static class JobConfigurator
{
    public static IServiceCollection SetupJobs(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        serviceCollection
            .AddScoped<IUserAlertsService, UserAlertsService>()
            .AddHttpClient()
            .AddScoped<IHttpClient, HttpClient>()
            .AddSingleton<ISession, Session>()
            .AddScoped<ExecuteAlertsCommandHandler>();

        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection.AddHostedService<ExecuteAlertsPeriodically>();

        return serviceCollection;
    }
}