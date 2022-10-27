using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SiteWatcher.Domain.Enums;
using Worker.Messaging;

namespace SiteWatcher.Worker.Jobs;

public sealed class FireWatchAlertsJob : IJob
{
    private readonly ILogger<FireWatchAlertsJob> _logger;
    private readonly ICapPublisher _capBus;
    private readonly WorkerSettings _settings;

    public static string Name => nameof(FireWatchAlertsJob);

    public FireWatchAlertsJob(ILogger<FireWatchAlertsJob> logger, ICapPublisher capBus, IOptions<WorkerSettings> settings)
    {
        _logger = logger;
        _capBus = capBus;
        _settings = settings.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var frequency = Enum.Parse<EFrequency>(context.Trigger.Key.Group);
        var message = new WatchAlertsMessage(frequency);
        await _capBus.PublishAsync(RoutingKeys.WatchAlerts, message);
        _logger.LogInformation("{Date} - Watch Alerts Fired: {Frequency}", DateTime.UtcNow, frequency);
    }
}