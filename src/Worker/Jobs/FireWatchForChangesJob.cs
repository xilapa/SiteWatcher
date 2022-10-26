using Microsoft.Extensions.Logging;
using Quartz;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Worker.Jobs;

public sealed class FireWatchForChangesJob : IJob
{
    private readonly ILogger<FireWatchForChangesJob> _logger;
    public static string Name => nameof(FireWatchForChangesJob);

    public FireWatchForChangesJob(ILogger<FireWatchForChangesJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        // TODO: use the trigger group to send a message to the queue
        var frequency = Enum.Parse<EFrequency>(context.Trigger.Key.Group);
        _logger.LogInformation($"Started {(int)frequency}");
        return Task.CompletedTask;
    }
}