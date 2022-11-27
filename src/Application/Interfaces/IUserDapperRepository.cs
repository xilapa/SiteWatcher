using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Application.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel?> GetUserAsync(string googleId, CancellationToken cancellationToken);
    Task<bool> DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken);
}