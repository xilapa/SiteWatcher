using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Messaging;

namespace SiteWatcher.Worker.Jobs;

public sealed class ExecuteAlertsPeriodically : BackgroundService
{
    private readonly ILogger<ExecuteAlertsPeriodically> _logger;
    private readonly SiteWatcherContext _context;
    private readonly ICapPublisher _capPublisher;
    private readonly ExecuteAlertsCommandHandler _handler;
    private readonly PeriodicTimer _timer;

    public ExecuteAlertsPeriodically(ILogger<ExecuteAlertsPeriodically> logger, SiteWatcherContext context, ICapPublisher capPublisher,
        ExecuteAlertsCommandHandler handler, IAppSettings settings)
    {
        _logger = logger;
        _context = context;
        _capPublisher = capPublisher;
        _handler = handler;

        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(2));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
         Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteAlerts(stoppingToken);
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }, stoppingToken);

    private async Task ExecuteAlerts(CancellationToken ct)
    {
        var frequencies = GetAlertFrequenciesForCurrentHour();

        _logger.LogInformation("{Date} - Watch Alerts Started: {Frequencies}", DateTime.UtcNow, frequencies);

        using var transaction = _context.Database.BeginTransaction(_capPublisher, autoCommit: false);

        var notifications = await _handler.Handle(new ExecuteAlertsCommand(frequencies), ct);

        foreach (var n in (notifications as ValueResult<List<NotificationToSend>>)!.Value)
        {
            // Publish the email message on the bus
            // Use the email id as the message id
            var headers = new Dictionary<string, string>
            {
                [MessageHeaders.MessageIdKey] = n.EmailNotification!.EmailId.ToString()!
            };
            var emailNotifMessage = new EmailNotificationMessage(n.EmailNotification!);
            await _capPublisher.PublishAsync(RoutingKeys.EmailNotification, emailNotifMessage, headers!, ct);
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        await transaction.CommitAsync(CancellationToken.None);

        _logger.LogInformation("{Date} - Watch Alerts Finished: {Frequencies}", DateTime.UtcNow, frequencies);
    }

    private static IEnumerable<Frequencies> GetAlertFrequenciesForCurrentHour()
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