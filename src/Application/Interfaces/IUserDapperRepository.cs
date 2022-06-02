using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel> GetUserAsync(string googleId, CancellationToken cancellationToken);
    Task<UserViewModel> GetInactiveUserAsync(UserId googleId, CancellationToken cancellationToken);
    Task DeleteActiveUserAsync(UserId userId);
}