using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Repositories;

// TODO: rename to IAlertCommandRepository
// TODO: remove generic repo interface
public interface IAlertRepository : IRepository<Alert>
{
    Task<Alert?> GetAlertForUpdate(AlertId alertId, UserId userId);
    void DeleteRule(RuleId ruleId);
}