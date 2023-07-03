using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Notifications;

namespace SiteWatcher.Application.Notifications.Commands.ProcessNotifications;

public class ProcessNotificationCommandHandler
{
    private readonly ISession _session;
    private readonly IAppSettings _appSettings;
    private readonly ISiteWatcherContext _context;

    public ProcessNotificationCommandHandler(ISession session, IAppSettings appSettings, ISiteWatcherContext context)
    {
        _session = session;
        _appSettings = appSettings;
        _context = context;
    }

    public async Task<CommandResult> Handle(AlertsTriggeredEvent @event, CancellationToken ct)
    {
        try
        {
            var notification = new Notification(@event, _session.Now, _appSettings.FrontEndAuthUrl);

            await notification.ProcessAndDispatch(_session.Now);

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(ct);
        }
        catch
        {
            return CommandResult.FromValue(false);
        }

        return CommandResult.FromValue(true);
    }
}