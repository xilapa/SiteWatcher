using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Messaging;

public sealed class Publisher : IPublisher
{
    private readonly ICapPublisher _capPublisher;

    public Publisher(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public Task PublishAsync(string routingKey, object message, Dictionary<string, string>? headers, CancellationToken ct)
    {
        headers ??= new Dictionary<string, string>(1);
        headers.Add("content-type","application/json");
        return _capPublisher.PublishAsync(routingKey, message, headers!, ct);
    }
}