using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Domain.Users.Repositories;

public interface IUserDapperRepository
{
    Task<UserViewModel?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken);
    Task<UserViewModel?> GetUserByIdAsync(UserId id, CancellationToken ct);
    Task<bool> DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken);
}