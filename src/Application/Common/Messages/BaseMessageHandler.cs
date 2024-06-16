using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Messages;

namespace SiteWatcher.Application.Common.Messages;

public abstract partial class BaseMessageHandler<T> : IConsumer<T> where T : BaseMessage
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

    public async Task Consume(ConsumeContext<T> context)
    {
        if (await Context.HasBeenProcessed(context.Message.Id, _consumerName))
        {
            // Exception to check if masstansit outbox is really idempotent as it claims to be
            throw new Exception(
                $"{Session.Now} Message with Id: {context.Message.Id} has already been processed by {_consumerName}");
        }

        await Handle(context);
        Context.MarkMessageAsConsumed(context.Message.Id, _consumerName, Session.Now);
        await Context.SaveChangesAsync(CancellationToken.None);

        LogMessageConsumed(Session.Now, context.Message.Id, _consumerName);
    }

    [LoggerMessage(LogLevel.Information, "{Date} Message with Id: {Message} has been processed by {Consumer}")]
    public partial void LogMessageConsumed(DateTime date, string message, string consumer);

    protected abstract Task Handle(ConsumeContext<T> context);
}