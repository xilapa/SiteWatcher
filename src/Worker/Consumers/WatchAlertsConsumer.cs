using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Worker.Messaging;

namespace SiteWatcher.Worker.Consumers;

public sealed class WatchAlertsConsumer : IWatchAlertsConsumer, ICapSubscribe
{
    private readonly ILogger<WatchAlertsConsumer> _logger;

    public WatchAlertsConsumer(ILogger<WatchAlertsConsumer> logger)
    {
        _logger = logger;
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(RoutingKeys.WatchAlerts, Group = RoutingKeys.WatchAlerts)]
    public async Task Consume(WatchAlertsMessage message, CancellationToken cancellationToken)
    {
        var messageJson = JsonSerializer.Serialize(message);
        _logger.LogInformation("{Date} Message received: {Message}", DateTime.UtcNow, messageJson);
    }
}

public interface IWatchAlertsConsumer
{
    Task Consume(WatchAlertsMessage message, CancellationToken cancellationToken);
}