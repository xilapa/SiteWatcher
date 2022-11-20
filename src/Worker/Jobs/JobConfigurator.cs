using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace SiteWatcher.Worker.Jobs;

public static class JobConfigurator
{
    public static IServiceCollection SetupJobs(this IServiceCollection serviceCollection, WorkerSettings settings, IHostEnvironment env)
    {
        if (!settings.EnableJobs)
            return serviceCollection;

        var fireWatchAlertsCron = env.IsDevelopment()
            ? "0 * * * * ?" // every minute
            : "0 0 */2 * * ?"; // every two hours

        serviceCollection
            .AddQuartz(opts =>
                 {
                     opts.UseMicrosoftDependencyInjectionJobFactory();

                     opts.AddJob<FireWatchAlertsJob>(opt => opt.WithIdentity(FireWatchAlertsJob.Name));

                    // Fire the job every two hours
                     opts.AddTrigger(opt => opt
                     .ForJob(FireWatchAlertsJob.Name)
                     .WithCronSchedule(fireWatchAlertsCron));
                 })
            // Wait to jobs to end gracefully on shutdown request
            .AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}