using Domain.DTOs.Alerts;
using Domain.DTOs.Common;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertDapperRepository
{
    Task<PaginatedList<SimpleAlertView>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken);

    Task<AlertDetailsDto?> GetAlertDetails(int alertId, UserId userId, CancellationToken cancellationToken);
    Task<bool> DeleteUserAlert(int alertId, UserId userId, CancellationToken cancellationToken);

    Task<List<SimpleAlertViewDto>> SearchSimpleAlerts(string[] searchTerms, UserId userId, int take,
        CancellationToken cancellationToken);
}