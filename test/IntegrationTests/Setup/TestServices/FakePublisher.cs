using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakePublisher : IPublisher
{
    public List<FakePublishedMessage> Messages { get; } = new();

    public Task PublishAsync(object message, CancellationToken ct)
    {
        Messages.Add(new FakePublishedMessage
        {
            Content = message,
        });
        return Task.CompletedTask;
    }
}

public sealed class FakePublishedMessage
{
    public string RoutingKey { get; set; } = null!;
    public object Content { get; set; } = null!;
}
