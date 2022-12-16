using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
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
        using var trx = _ctx.Database.BeginTransaction(_capPub, autoCommit: false);

        await func(new Publisher(_capPub));

        await _ctx.SaveChangesAsync(ct);
        await trx.CommitAsync(CancellationToken.None);
    }
}