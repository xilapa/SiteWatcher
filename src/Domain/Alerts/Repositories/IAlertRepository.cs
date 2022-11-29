using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Repositories;

// TODO: rename to IAlertCommandRepository
// TODO: remove generic repo interface
public interface IAlertRepository : IRepository<Alert>
{
    Task<Alert?> GetAlertForUpdate(AlertId alertId, UserId userId);
    void DeleteWatchMode(WatchModeId watchModeId);
}