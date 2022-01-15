using System;
using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.Domain.Entities;
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

    public async Task Subscribe(UserSubscribeIM userSubscribe)
    {
        if (userSubscribe.Email is null) throw new ArgumentNullException(nameof(userSubscribe.Email));

        var userToSubscribe = await this.userRepository.FindAsync(u => u.Email == userSubscribe.Email);

        if(userToSubscribe is null)
        {
            userToSubscribe =  await this.userService.AddUser(
                new User(userSubscribe.Name, userSubscribe.Email)
            );
        }

        this.userService.SubscribeUser(userToSubscribe);
        await this.userRepository.UoW.SaveChangesAsync();
    }
}