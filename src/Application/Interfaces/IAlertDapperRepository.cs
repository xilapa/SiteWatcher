using Domain.DTOs.Alert;
using Domain.DTOs.Common;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertDapperRepository
{
    Task<PaginatedList<SimpleAlertViewDto>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken);

    Task<AlertDetailsDto?> GetAlertDetails(int alertId, UserId userId, CancellationToken cancellationToken);
    Task<bool> DeleteUserAlert(int alertId, UserId userId, CancellationToken cancellationToken);

    Task<List<SimpleAlertViewDto>> SearchSimpleAlerts(string[] searchTerms, UserId userId, int take,
        CancellationToken cancellationToken);
}