using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Users.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> GetUserWithPendingAlertsAsync(IEnumerable<Frequencies> frequencies, int take, DateTime? lastCreatedAt, CancellationToken ct);
}