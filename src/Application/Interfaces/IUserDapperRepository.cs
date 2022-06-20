using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel> GetUserAsync(string googleId, CancellationToken cancellationToken);
    Task<bool> DeleteActiveUserAsync(UserId userId, CancellationToken cancellationToken);
}