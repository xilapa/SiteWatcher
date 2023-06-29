using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Alerts.EventHandlers;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Events;
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
            .AddHttpClient()
            .AddScoped<IHttpClient, HttpClient>()
            .AddSingletonSession()
            .AddScoped<ExecuteAlertsCommandHandler>()
            // Adding MediatR with only the handler that is used by the worker
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(IMediator).Assembly))
            .AddScoped<INotificationHandler<AlertsTriggeredEvent>, AlertsTriggeredEventHandler>();

        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection.AddHostedService<ExecuteAlertsPeriodically>();

        return serviceCollection;
    }
}