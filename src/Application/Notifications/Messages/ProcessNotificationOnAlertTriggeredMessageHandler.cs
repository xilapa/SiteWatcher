using MassTransit;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Notifications;

namespace SiteWatcher.Application.Notifications.Messages;

public sealed class ProcessNotificationOnAlertTriggeredMessageHandler : BaseMessageHandler<AlertsTriggeredMessage>
{
    private readonly IAppSettings _settings;

    public ProcessNotificationOnAlertTriggeredMessageHandler(ISiteWatcherContext context,
        ILogger<AlertsTriggeredMessage> logger, ISession session, IAppSettings settings) : base(context, logger, session)
    {
        _settings = settings;
    }

    protected override async Task Handle(ConsumeContext<AlertsTriggeredMessage> context)
    {
        var notification = new Notification(context.Message, Session.Now, _settings.FrontEndAuthUrl);

        await notification.ProcessAndDispatch(Session.Now);

        Context.Notifications.Add(notification);

        await Context.SaveChangesAsync(CancellationToken.None);
    }
}