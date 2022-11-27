using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertRepository : IRepository<Alert>
{
    Task<Alert?> GetAlertForUpdate(AlertId alertId, UserId userId);
    void DeleteWatchMode(WatchModeId watchModeId);
}