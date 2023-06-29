using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Notifications.Commands.ProcessNotifications;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Persistence;

namespace SiteWatcher.Worker.Consumers;

public class AlertsTriggeredEventConsumer : ICapSubscribe
{
    private readonly SiteWatcherContext _ctx;
    private readonly ILogger<AlertsTriggeredEventConsumer> _logger;
    private readonly ProcessNotificationCommandHandler _handler;

    public AlertsTriggeredEventConsumer(SiteWatcherContext ctx, ILogger<AlertsTriggeredEventConsumer> logger,
        ProcessNotificationCommandHandler handler)
    {
        _ctx = ctx;
        _logger = logger;
        _handler = handler;
    }

    [CapSubscribe(RoutingKeys.AlertsTriggered, Group = RoutingKeys.AlertsTriggered)]
    public async Task Consume(AlertsTriggeredEvent message, CancellationToken ct)
    {
        var hasBeenProcessed = await _ctx.HasBeenProcessed(message.Id, nameof(AlertsTriggeredEventConsumer));
        if (hasBeenProcessed)
        {
            _logger.LogInformation("{Date} AlertsTriggeredEvent has already been processed: {MessageId}", DateTime.UtcNow, message.Id);
            return;
        }

        await using var transaction = await _ctx.Database.BeginTransactionAsync(ct);
        _ctx.MarkMessageAsConsumed(message.Id, nameof(AlertsTriggeredEventConsumer));
        var res = await _handler.Handle(message, ct) as ValueResult<bool>;

        if (!res!.Value) throw new Exception($"Cant process AlertsTriggeredEvent {message.Id}");

        _logger.LogInformation("{Date} AlertsTriggeredEvent consumed: {MessageId}", DateTime.UtcNow, message.Id);
    }
}