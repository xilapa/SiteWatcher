using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertRepository : IRepository<Alert>
{
    Task<Alert?> GetAlertForUpdate(AlertId alertId, UserId userId);
    void DeleteWatchMode(WatchModeId watchModeId);
}