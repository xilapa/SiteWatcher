namespace SiteWatcher.Application.Interfaces;

public interface IUserService
{
    Domain.Models.User Register(Domain.Models.User user);
}