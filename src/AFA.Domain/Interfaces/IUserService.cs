using System.Threading.Tasks;
using AFA.Domain.Entities;

namespace AFA.Domain.Interfaces;

public interface IUserService
{
    Task<User> AddUser(User userInput);
    User SubscribeUser(User user);
}