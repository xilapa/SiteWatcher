using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Application.Interfaces;

public interface IUserDapperRepository
{
    Task<UserViewModel> GetActiveUserAsync(string googleId);
}