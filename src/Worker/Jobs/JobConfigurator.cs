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

                    // Fire the job every two hours
                     opts.AddTrigger(opt => opt
                     .ForJob(FireWatchAlertsJob.Name)
                     .WithCronSchedule("0 0 */2 * * ?"));
                 })
            // Wait to jobs to end gracefully on shutdown request
            .AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}