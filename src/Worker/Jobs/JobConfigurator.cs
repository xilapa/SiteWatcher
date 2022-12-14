using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Infra.Authorization;
using HttpClient = SiteWatcher.Infra.Http.HttpClient;

namespace SiteWatcher.Worker.Jobs;

public static class JobConfigurator
{
    public static IServiceCollection SetupJobs(this IServiceCollection serviceCollection, WorkerSettings settings, IHostEnvironment env)
    {
        serviceCollection
            .AddScoped<IUserAlertsService, UserAlertsService>()
            .AddHttpClient()
            .AddScoped<IHttpClient, HttpClient>()
            .AddSingleton<ISession, Session>()
            .AddSingleton<IAppSettings>(new WorkerAppSettings
            {
                IsDevelopment = env.IsDevelopment(),
                ConnectionString = settings.DbConnectionString,
                FrontEndUrl = settings.SiteWatcherUri
            })
            .AddScoped<ExecuteAlertsCommandHandler>();

        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection.AddHostedService<ExecuteAlertsPeriodically>();

        return serviceCollection;
    }
}