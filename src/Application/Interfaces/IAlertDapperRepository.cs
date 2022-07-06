using Domain.DTOs.Alert;
using Domain.DTOs.Common;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertDapperRepository
{
    Task<PaginatedList<SimpleAlertViewDto>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken);
}