using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Notifications;

namespace SiteWatcher.Application.Notifications.Messages;

public class ProcessNotificationOnAlertTriggeredMessageHandler : BaseMessageHandler<AlertsTriggeredMessage>
{
    private readonly IAppSettings _settings;
    private readonly IPublisher _publisher;

    public ProcessNotificationOnAlertTriggeredMessageHandler(ISiteWatcherContext context,
        ILogger<AlertsTriggeredMessage> logger, ISession session, IAppSettings settings, IPublisher publisher) : base(context, logger, session)
    {
        _settings = settings;
        _publisher = publisher;
    }

    protected override async Task Handle(ConsumeContext<AlertsTriggeredMessage> context)
    {
        var notification = new Notification(context.Message, Session.Now, _settings.FrontEndAuthUrl);

        var emailCreatedMessage = await notification.ProcessAndDispatch(Session.Now);

        Context.Notifications.Add(notification);
        await _publisher.PublishAsync(emailCreatedMessage, context.CancellationToken);

        await Context.SaveChangesAsync(CancellationToken.None);
    }
}