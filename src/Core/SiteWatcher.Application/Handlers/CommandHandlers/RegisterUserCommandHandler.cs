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
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IUnityOfWork _uow;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(IUserService userService, IMapper mapper, IMediator mediator, IUnityOfWork uow, ITokenService tokenService)
    {
        this._userService = userService;
        this._mapper = mapper;
        this._mediator = mediator;
        this._uow = uow;
        this._tokenService = tokenService;
    }

    public async Task<ApplicationResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request);
        var appResult = new ApplicationResult<string>();

        try
        {
            _userService.Register(user);
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch(UniqueViolationException ex)
        {
            return appResult.AddError(ex.Message);
        }

        var token = _tokenService.GenerateLoginToken(user);

        if(!user.EmailConfirmed)
            await _mediator.Publish(_mapper.Map<UserRegisteredNotification>(user), cancellationToken);

        return appResult.SetValue(token);
    }
}