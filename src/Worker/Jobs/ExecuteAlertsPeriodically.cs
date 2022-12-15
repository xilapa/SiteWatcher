using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExecuteAlertsPeriodically> _logger;
    private readonly ICapPublisher _capPublisher;
    private readonly PeriodicTimer _timer;

    public ExecuteAlertsPeriodically(IServiceScopeFactory scopeProvider, ILogger<ExecuteAlertsPeriodically> logger, ICapPublisher capPublisher, IAppSettings settings)
    {
        _scopeFactory = scopeProvider;
        _logger = logger;
        _capPublisher = capPublisher;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
         Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await using var ctx = scope.ServiceProvider.GetRequiredService<SiteWatcherContext>();
                var handler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
                await ExecuteAlerts(ctx, handler, stoppingToken);
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }, stoppingToken);

    private async Task ExecuteAlerts(SiteWatcherContext ctx, ExecuteAlertsCommandHandler handler, CancellationToken ct)
    {
        var frequencies = GetAlertFrequenciesForCurrentHour();

        _logger.LogInformation("{Date} - Watch Alerts Started: {Frequencies}", DateTime.UtcNow, frequencies);

        using var transaction = ctx.Database.BeginTransaction(_capPublisher, autoCommit: false);

        var notifications = await handler.Handle(new ExecuteAlertsCommand(frequencies), ct);

        foreach (var n in (notifications as ValueResult<List<NotificationToSend>>)!.Value)
        {
            // Publish the email message on the bus
            // Use the email id as the message id
            var headers = new Dictionary<string, string>
            {
                [MessageHeaders.MessageIdKey] = n.EmailNotification!.EmailId.ToString()!,
                ["content-type"] = "application/json"
            };
            var emailNotifMessage = new EmailNotificationMessage(n.EmailNotification!);
            await _capPublisher.PublishAsync(RoutingKeys.EmailNotification, emailNotifMessage, headers!, ct);
        }

        await ctx.SaveChangesAsync(CancellationToken.None);

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