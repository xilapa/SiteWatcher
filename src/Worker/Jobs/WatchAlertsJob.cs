using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Messaging;

namespace SiteWatcher.Worker.Jobs;

public sealed class WatchAlertsJob : IJob
{
    private readonly ILogger<WatchAlertsJob> _logger;
    private readonly SiteWatcherContext _context;
    private readonly ICapPublisher _capPublisher;
    private readonly ExecuteAlertsCommandHandler _handler;

    public static string Name => nameof(WatchAlertsJob);

    public WatchAlertsJob(ILogger<WatchAlertsJob> logger, SiteWatcherContext context, ICapPublisher capPublisher,
        ExecuteAlertsCommandHandler handler)
    {
        _logger = logger;
        _context = context;
        _capPublisher = capPublisher;
        _handler = handler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var frequencies = Enum.GetValues<Frequencies>();

        _logger.LogInformation("{Date} - Watch Alerts Started: {Frequencies}", DateTime.UtcNow, frequencies);

        using var transaction = _context.Database.BeginTransaction(_capPublisher, autoCommit: false);

        var notifications = await _handler.Handle(new ExecuteAlertsCommand(frequencies), context.CancellationToken);

        foreach (var n in (notifications as ValueResult<List<NotificationToSend>>)!.Value)
        {
            // Publish the email message on the bus
            // Use the email id as the message id
            var headers = new Dictionary<string, string>
            {
                [MessageHeaders.MessageIdKey] = n.EmailNotification!.EmailId.ToString()!
            };
            var emailNotifMessage = new EmailNotificationMessage(n.EmailNotification!);
            await _capPublisher.PublishAsync(RoutingKeys.EmailNotification, emailNotifMessage, headers!, context.CancellationToken);
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        await transaction.CommitAsync(CancellationToken.None);

        _logger.LogInformation("{Date} - Watch Alerts Finished: {Frequencies}", DateTime.UtcNow, frequencies);
    }

    private static IEnumerable<Frequencies> GetAlertFrequenciesToWatch()
    {
        var alertFrequenciesToWatch = new List<Frequencies>();

        // TODO: Wrapper for time, to make tests possible
        var currentHour = DateTime.Now.Hour;

        // If the rest of current hour/ frequency is zero, this alerts of this frequency needs to be watched
        foreach (var frequency in Enum.GetValues<Frequencies>())
        {
            if (currentHour % (int)frequency == 0)
                alertFrequenciesToWatch.Add(frequency);
        }

        return alertFrequenciesToWatch;
    }
}