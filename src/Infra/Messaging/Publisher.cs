using DotNetCore.CAP;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Messaging;

public sealed class Publisher : IPublisher
{
    private readonly SiteWatcherContext _ctx;
    private readonly ICapPublisher _capPublisher;

    public Publisher(SiteWatcherContext ctx, ICapPublisher capPublisher)
    {
        _ctx = ctx;
        _capPublisher = capPublisher;
    }

    public async Task PublishAsync(string routingKey, object message, Dictionary<string, string>? headers, CancellationToken ct)
    {
        await using var trx = await _ctx.Database.BeginTransactionAsync(_capPublisher, autoCommit: false, cancellationToken: ct);
        headers ??= new Dictionary<string, string>(1);
        headers.Add("content-type","application/json");
        await _capPublisher.PublishAsync(routingKey, message, headers!, ct);
    }

    public Task PublishAsync(string routingKey, object message, CancellationToken ct)
    {
        return PublishAsync(routingKey, message, null, ct);
    }
}