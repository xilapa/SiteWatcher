using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public User Register(User user) =>    
        userRepository.Add(user);

}