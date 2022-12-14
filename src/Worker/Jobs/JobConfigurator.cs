using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
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

        var fireWatchAlertsCron = env.IsDevelopment()
            ? "0 * * * * ?" // every minute
            : "0 0 */2 * * ?"; // every two hours

        serviceCollection
            .AddQuartz(opts =>
                 {
                     opts.UseMicrosoftDependencyInjectionJobFactory();

                     opts.AddJob<WatchAlertsJob>(opt => opt.WithIdentity(WatchAlertsJob.Name));

                     // Fire the job every two hours
                     opts.AddTrigger(opt => opt
                     .ForJob(WatchAlertsJob.Name)
                     .WithCronSchedule(fireWatchAlertsCron));

                     // Run on worker startup
                     opts.AddTrigger(opt => opt
                      .ForJob(WatchAlertsJob.Name)
                      .StartNow());
                 })
            // Wait to jobs to end gracefully on shutdown request
            .AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}