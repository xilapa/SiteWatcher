using AutoMapper;
using MediatR;
using SiteWatcher.Application.Commands;
using SiteWatcher.Application.Metadata;
using SiteWatcher.Application.Notifications;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Handlers.CommandHandlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApplicationResult<string>>
{
    private readonly IUserService userService;
    private readonly IMapper mapper;
    private readonly IMediator mediator;
    private readonly IUnityOfWork uow;
    private readonly ITokenService tokenService;

    public RegisterUserCommandHandler(IUserService userService, IMapper mapper, IMediator mediator, IUnityOfWork uow, ITokenService tokenService)
    {
        this.userService = userService;
        this.mapper = mapper;
        this.mediator = mediator;
        this.uow = uow;
        this.tokenService = tokenService;
    }

    public async Task<ApplicationResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<User>(request);
        var appResult = new ApplicationResult<string>();

        try
        {
            userService.Register(user);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch(UniqueViolationException ex)
        {
            return appResult.AddError(ex.Message);
        }

        var token = tokenService.GenerateLoginToken(user);

        if(!user.EmailConfirmed)
            await mediator.Publish(mapper.Map<UserRegisteredNotification>(user), cancellationToken);

        return appResult.SetValue(token);
    }
}