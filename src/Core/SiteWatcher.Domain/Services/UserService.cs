using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository) =>    
        this._userRepository = userRepository;

    public User Register(User user) =>
        _userRepository.Add(user);
}