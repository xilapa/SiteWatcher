using Microsoft.Extensions.Logging;
using Quartz;

namespace SiteWatcher.Worker.Jobs;

public sealed class FireWatchForChangesJob : IJob
{
    private readonly ILogger<FireWatchForChangesJob> _logger;

    public FireWatchForChangesJob(ILogger<FireWatchForChangesJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        // TODO: use the trigger key to send a message to the queue
        _logger.LogInformation($"Started {context.Trigger.Key.Name}");
        return Task.CompletedTask;
    }
}