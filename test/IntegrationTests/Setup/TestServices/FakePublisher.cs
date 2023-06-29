using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakePublisher : IPublisher
{
    public List<FakePublishedMessage> Messages { get; } = new();

    public Task PublishAsync(string routingKey, object message, Dictionary<string, string>? headers, CancellationToken ct)
    {
        Messages.Add(new FakePublishedMessage
        {
            RoutingKey = routingKey,
            Content = message,
            Headers = headers
        });
        return Task.CompletedTask;
    }

    public Task PublishAsync(string routingKey, object message, CancellationToken ct)
    {
        return PublishAsync(routingKey, message, null!, ct);
    }
}

public sealed class FakePublishedMessage
{
    public string RoutingKey { get; set; } = null!;
    public object Content { get; set; } = null!;
    public Dictionary<string, string>? Headers { get; set; }
}

public sealed class FakePublishService : IPublishService
{
    private readonly IPublisher _publisher;

    public FakePublishService(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task WithPublisher(Func<IPublisher, Task> func, CancellationToken ct) =>
        await func(_publisher);
}
