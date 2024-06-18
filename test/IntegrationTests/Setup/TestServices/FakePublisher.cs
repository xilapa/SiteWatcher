using MassTransit;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakePublisher : IPublishEndpoint
{
    public List<FakePublishedMessage> Messages { get; } = new();

    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
    {
        Messages.Add(new FakePublishedMessage
        {
            Content = message,
        });
        return Task.CompletedTask;
    }

    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }
}

public sealed class FakePublishedMessage
{
    public object Content { get; set; } = null!;
}
