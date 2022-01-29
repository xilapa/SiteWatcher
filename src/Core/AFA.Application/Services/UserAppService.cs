using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.Domain.Interfaces;
using AFA.Application.Validators;
using AFA.Application.DTOS.Metadata;
using AFA.Domain.Extensions;
using AFA.Domain.Enums;
using AFA.Domain.Entities;

namespace AFA.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserService userService;
    private readonly IUserRepository userRepository;
    private readonly IFireForgetService fireForgetService;

    public UserAppService(IUserService userService, IUserRepository userRepository, IFireForgetService fireForgetService)
    {
        this.userService = userService;
        this.userRepository = userRepository;
        this.fireForgetService = fireForgetService;
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

        // Teste FireForget Criar user aleatorio
        fireForgetService.ExecuteWith<IUserRepository>(userRepo =>
        {
            userRepo.Add(new User("FIREFORGET", "FIREFORGET@email.com") { Subscribed = true});
            return userRepo.UoW.SaveChangesAsync();
        });

        return new ApplicationResponseOf<ESubscriptionResult>(subscribeResult.GetDescription(), subscribeResult);
    }
}