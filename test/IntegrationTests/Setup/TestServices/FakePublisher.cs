using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakePublisher : IPublisher
{
    public List<FakePublishedMessage> Messages { get; } = new();

    public Task PublishAsync(string routingKey, object message, Dictionary<string, string> headers, CancellationToken ct)
    {
        Messages.Add(new FakePublishedMessage
        {
            RoutingKey = routingKey,
            Message = message,
            Headers = headers
        });
        return Task.CompletedTask;
    }
}

public sealed class FakePublishedMessage
{
    public string RoutingKey { get; set; } = null!;
    public object Message { get; set; } = null!;
    public Dictionary<string, string> Headers { get; set; } = null!;
}

public sealed class FakePublishService : IPublishService
{
    private readonly SiteWatcherContext _ctx;
    private readonly IPublisher _publisher;

    public FakePublishService(SiteWatcherContext ctx, IPublisher publisher)
    {
        _ctx = ctx;
        _publisher = publisher;
    }

    public async Task WithPublisher(Func<IPublisher, Task> func, CancellationToken ct)
    {
        await func(_publisher);

        await _ctx.SaveChangesAsync(ct);
    }
}
