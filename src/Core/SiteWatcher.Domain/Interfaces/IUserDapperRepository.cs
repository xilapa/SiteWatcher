using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Domain.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel> GetActiveUserAsync(string googleId);
}