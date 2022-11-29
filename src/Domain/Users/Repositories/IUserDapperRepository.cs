using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Domain.Users.Repositories;

public interface IUserDapperRepository
{
    Task<UserViewModel?> GetUserAsync(string googleId, CancellationToken cancellationToken);
    Task<bool> DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken);
}