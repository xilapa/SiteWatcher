using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Repositories;

// TODO: rename to IAlertQueryRepository
public interface IAlertDapperRepository
{
    Task<PaginatedList<SimpleAlertView>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken);

    Task<AlertDetails?> GetAlertDetails(int alertId, UserId userId, CancellationToken cancellationToken);
    Task<bool> DeleteUserAlert(int alertId, UserId userId, CancellationToken cancellationToken);

    Task<IEnumerable<SimpleAlertView>> SearchSimpleAlerts(string[] searchTerms, UserId userId, int take,
        CancellationToken cancellationToken);
}