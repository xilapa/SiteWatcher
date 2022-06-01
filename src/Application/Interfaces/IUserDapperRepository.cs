using SiteWatcher.Domain.DTOs.User;

namespace SiteWatcher.Application.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel> GetActiveUserAsync(string googleId);
}