using System;
using System.Linq;
using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.Domain.Entities;
using AFA.Domain.Interfaces;

namespace AFA.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserService userService;
    private readonly AFAContext context;
    public UserAppService(IUserService userService, AFAContext context)
    {
        this.userService = userService;
        this.context = context;
    }

    public async Task Subscribe(UserSubscribeIM userSubscribe)
    {
        if (userSubscribe.Email is null) throw new ArgumentNullException(nameof(userSubscribe.Email));

        var userToSubscribe = this.context.Users.SingleOrDefault(u => u.Email == userSubscribe.Email);

        if(userToSubscribe is null)
        {
            userToSubscribe =  await this.userService.AddUser(
                new User { Name = userSubscribe.Name, Email = userSubscribe.Email }
            );
        }

        this.userService.SubscribeUser(userToSubscribe);
        await this.context.SaveChangesAsync();
    }
}