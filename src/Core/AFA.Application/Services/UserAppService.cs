using System.Threading.Tasks;
using AFA.Application.Interfaces;
using AFA.Application.DTOS.InputModels;
using AFA.Domain.Interfaces;
using AFA.Application.Validators;
using AFA.Application.DTOS.Metadata;
using AFA.Domain.Extensions;

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

    public async Task<ApplicationResponse> Subscribe(UserSubscribeIM userSubscribeIM)
    {
        var inputValidation = userSubscribeIM.Validate();

        if(!inputValidation.IsValid)
            return new ApplicationResponse(inputValidation);

        var userToSubscribe = await this.userRepository.GetAsync(u => u.Email == userSubscribeIM.Email);

        if(userToSubscribe is null)        
            userToSubscribe = await this.userService.CreateUser(userSubscribeIM.Name, userSubscribeIM.Email);        

        var subscribeResult = await this.userService.SubscribeUser(userToSubscribe.Id);

        await this.userRepository.UoW.SaveChangesAsync();

        // TODO: enviar email para confirmar a inscrição

        return new ApplicationResponse(subscribeResult.GetDescription());
    }
}