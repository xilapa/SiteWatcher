using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Persistence;

namespace SiteWatcher.Worker.Consumers;

public sealed class WatchAlertsConsumer : IWatchAlertsConsumer, ICapSubscribe
{
    private readonly ILogger<WatchAlertsConsumer> _logger;
    private readonly SiteWatcherContext _context;

    public WatchAlertsConsumer(ILogger<WatchAlertsConsumer> logger, SiteWatcherContext context)
    {
        _logger = logger;
        _context = context;
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(RoutingKeys.WatchAlerts, Group = RoutingKeys.WatchAlerts)]
    public async Task Consume(WatchAlertsMessage message, [FromCap]CapHeader capHeader, CancellationToken cancellationToken)
    {
        var hasBeenProcessed = await _context
            .HasBeenProcessed(capHeader[MessageHeaders.MessageIdKey]!, nameof(WatchAlertsConsumer));

        if (hasBeenProcessed)
        {
            _logger.LogInformation("{Date} Message has already been processed: {MessageId}", DateTime.UtcNow, capHeader[MessageHeaders.MessageIdKey]!);
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await GenerateNotifications(message);
        await _context.MarkAsConsumed(capHeader[MessageHeaders.MessageIdKey]!, nameof(WatchAlertsConsumer));

        await transaction.CommitAsync(cancellationToken);

        var messageJson = JsonSerializer.Serialize(message);
        _logger.LogInformation("{Date} Message received: {Message}", DateTime.UtcNow, messageJson);
    }

    private async Task GenerateNotifications(WatchAlertsMessage message)
    {
        await Task.Delay(500);
    }
}

public interface IWatchAlertsConsumer
{
    Task Consume(WatchAlertsMessage message, CapHeader capHeader, CancellationToken cancellationToken);
}