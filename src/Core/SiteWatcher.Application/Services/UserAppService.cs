using System.Threading.Tasks;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.DTOs.InputModels;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Application.Validators;
using SiteWatcher.Application.DTOs.Metadata;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserService userService;
    private readonly IUserRepository userRepository;

    public UserAppService(IUserService userService, IUserRepository userRepository)
    {
        this.userService = userService;
        this.userRepository = userRepository;
    }

    public async Task<ApplicationResponseOf<ESubscriptionResult>> Subscribe(UserSubscribeIM userSubscribeIM)
    {
        var inputValidation = userSubscribeIM.Validate();

        if(!inputValidation.IsValid)
            return new ApplicationResponseOf<ESubscriptionResult>(inputValidation);

        var userToSubscribe = await this.userRepository.GetAsync(u => u.Email == userSubscribeIM.Email);

        if(userToSubscribe is null)        
            userToSubscribe = await this.userService.CreateUser(userSubscribeIM.Name, userSubscribeIM.Email);        

        var subscribeResult = await this.userService.SubscribeUser(userToSubscribe.Id);

        await this.userRepository.UoW.SaveChangesAsync();

        // TODO: enviar email para confirmar a inscrição

        return new ApplicationResponseOf<ESubscriptionResult>(subscribeResult.GetDescription(), subscribeResult);
    }
}