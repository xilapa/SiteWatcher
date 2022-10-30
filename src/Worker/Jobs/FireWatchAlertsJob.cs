using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Worker.Messaging;

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
        var frequencies = GetAlertFrequenciesToWatch();
        var message = new WatchAlertsMessage(frequencies);

        var headers = new Dictionary<string, string>
        {
            [MessageHeaders.MessageIdKey] = Guid.NewGuid().ToString()
        };
        await _capBus.PublishAsync(RoutingKeys.WatchAlerts, message, headers!);

        _logger.LogInformation("{Date} - Watch Alerts Fired: {Frequencies}", DateTime.UtcNow, frequencies);
    }

    private static IEnumerable<EFrequency> GetAlertFrequenciesToWatch()
    {
        var alertFrequenciesToWatch = new List<EFrequency>();

        var currentHour = DateTime.Now.Hour;

        // If the rest of current hour/ frequency is zero, this alerts of this frequency needs to be watched
        foreach(var frequency in Enum.GetValues<EFrequency>())
        {
            if (currentHour % (int)frequency == 0)
                alertFrequenciesToWatch.Add(frequency);
        }

        return alertFrequenciesToWatch;
    }
}