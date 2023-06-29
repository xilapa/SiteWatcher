using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Notifications;
using SiteWatcher.Domain.Notifications.Repositories;

namespace SiteWatcher.Application.Notifications.Commands.ProcessNotifications;

public class ProcessNotificationCommandHandler
{
    private readonly ISession _session;
    private readonly IAppSettings _appSettings;
    private readonly INotificationRepository _repo;
    private readonly IUnitOfWork _uow;

    public ProcessNotificationCommandHandler(ISession session, IAppSettings appSettings, INotificationRepository repo,
        IUnitOfWork uow)
    {
        _session = session;
        _appSettings = appSettings;
        _repo = repo;
        _uow = uow;
    }

    public async Task<CommandResult> Handle(AlertsTriggeredEvent @event, CancellationToken ct)
    {
        try
        {
            var notification = new Notification(@event, _session.Now, _appSettings.FrontEndAuthUrl);

            await notification.ProcessAndDispatch(_session.Now);

            _repo.Add(notification);

            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            return CommandResult.FromValue(false);
        }

        return CommandResult.FromValue(true);
    }
}