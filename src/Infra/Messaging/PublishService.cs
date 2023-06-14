using DotNetCore.CAP;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Messaging;

public sealed class PublishService : IPublishService
{
    private readonly SiteWatcherContext _ctx;
    private readonly ICapPublisher _capPub;

    public PublishService(SiteWatcherContext ctx, ICapPublisher capPub)
    {
        _ctx = ctx;
        _capPub = capPub;
    }

    public async Task WithPublisher(Func<IPublisher, Task> func, CancellationToken ct)
    {
        await using var trx = await _ctx.Database.BeginTransactionAsync(_capPub, autoCommit: false, cancellationToken: ct);

        await func(new Publisher(_capPub));
    }
}