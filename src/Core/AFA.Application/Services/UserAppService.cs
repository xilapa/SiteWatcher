using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.Domain.Interfaces;

namespace AFA.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserService userService;
    private readonly IUserRepository userRepository;

    public UserAppService(IUserService userService, IUserRepository userRepository)
    {
        this.userService = userService;
        this.userRepository = userRepository;
    }

    public async Task Subscribe(UserSubscribeIM userSubscribeIM)
    {
        var userToSubscribe = await this.userRepository.GetAsync(u => u.Email == userSubscribeIM.Email);

        if(userToSubscribe is null)        
            userToSubscribe = await this.userService.CreateUser(userSubscribeIM.Name, userSubscribeIM.Email);        

        await this.userService.SubscribeUser(userToSubscribe.Id);

        await this.userRepository.UoW.SaveChangesAsync();
    }
}