using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Users.Repositories;

public interface IUserRepository : IRepository<User>
{
    IAsyncEnumerable<User> GetUserWithAlertsAsync(IEnumerable<Frequencies> frequencies, CancellationToken ct);
}