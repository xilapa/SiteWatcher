using SiteWatcher.Domain.Models;

namespace SiteWatcher.Domain.Interfaces;

public interface IUserService
{
    User Register(User user);
}