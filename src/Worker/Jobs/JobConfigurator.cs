using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SiteWatcher.Worker.Jobs;

public static class JobConfigurator
{
    public static IServiceCollection SetupJobs(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection
            .AddQuartz(opts =>
                 {
                     opts.UseMicrosoftDependencyInjectionJobFactory();

                     opts.AddJob<FireWatchAlertsJob>(opt => opt.WithIdentity(FireWatchAlertsJob.Name));

                     foreach (var (freq, cron) in settings.Triggers)
                     {
                         opts.AddTrigger(opt => opt
                            .ForJob(FireWatchAlertsJob.Name)
                            // Store the alert frequency on group
                            .WithIdentity(FireWatchAlertsJob.Name,freq.ToString())
                            .WithCronSchedule(cron));
                     }
                 })
            // Wait to jobs to end gracefully on shutdown request
            .AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}