using Microsoft.Extensions.DependencyInjection;
using Quartz;
using static SiteWatcher.Worker.Jobs.Constants;

namespace SiteWatcher.Worker.Jobs;

public static class JobDependencyInjection
{
    public static IServiceCollection AddJobs(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        if (!settings.EnableJobs)
            return serviceCollection;

        serviceCollection
            .AddQuartz(opts =>
                 {
                     opts.UseMicrosoftDependencyInjectionJobFactory();

                     opts.AddJob<FireWatchForChangesJob>(opt => opt.WithIdentity(FireWatchForChangesKey));

                     // Trigger every two hours
                     opts.AddTrigger(opt => opt
                         .ForJob(FireWatchForChangesKey)
                         .WithIdentity(WatchForChangesEveryTwoHours)
                         .WithCronSchedule("0 0 */2 * * ?"));

                     // Trigger every four hours
                     opts.AddTrigger(opt => opt
                         .ForJob(FireWatchForChangesKey)
                         .WithIdentity(WatchForChangesEveryFourHours)
                         .WithCronSchedule("0 0 */4 * * ?"));

                     // Trigger every eight hours
                     opts.AddTrigger(opt => opt
                         .ForJob(FireWatchForChangesKey)
                         .WithIdentity(WatchForChangesEveryEightHours)
                         .WithCronSchedule("0 0 */8 * * ?"));

                     // Trigger every twelve hours
                     opts.AddTrigger(opt => opt
                         .ForJob(FireWatchForChangesKey)
                         .WithIdentity(WatchForChangesEveryTwelveHours)
                         .WithCronSchedule("0 0 */12 * * ?"));

                     // Trigger every twenty four hours
                     opts.AddTrigger(opt => opt
                         .ForJob(FireWatchForChangesKey)
                         .WithIdentity(WatchForChangesEveryTwentyFourHours)
                         .WithCronSchedule("0 0 0 * * ?"));
                 })
            // Wait to jobs to end gracefully on shutdown request
            .AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

        return serviceCollection;
    }
}