using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Messages;

namespace SiteWatcher.Application.Common.Messages;

public abstract class BaseMessageHandler<T> : IMessageHandler<T> where T : BaseMessage
{
    protected readonly ISiteWatcherContext Context;
    private readonly ILogger _logger;
    protected readonly ISession Session;
    private readonly string _consumerName;

    protected BaseMessageHandler(ISiteWatcherContext context, ILogger<T> logger, ISession session)
    {
        Context = context;
        _logger = logger;
        Session = session;
        _consumerName = GetType().Name;
    }

    public async Task Handle(T message, CancellationToken ct)
    {
        if (await Context.HasBeenProcessed(message.Id, _consumerName))
        {
            _logger.LogInformation("{Date} Message with Id: {Message} has already been processed by {Consumer}",
                Session.Now, message.Id, _consumerName);
            return;
        }

        Context.MarkMessageAsConsumed(message.Id, _consumerName);
        await Consume(message, ct);

        _logger.LogInformation("{Date} Message with Id: {Message} has been processed by {Consumer}",
            Session.Now, message.Id, _consumerName);
    }

    protected abstract Task Consume(T message, CancellationToken ct);
}