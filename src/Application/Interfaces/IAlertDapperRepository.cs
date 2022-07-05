using Domain.DTOs.Alert;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAlertDapperRepository
{
    Task<IEnumerable<SimpleAlertViewDto>> GetUserAlerts(UserId userId, int take, int lastAlertId,
        CancellationToken cancellationToken);
}