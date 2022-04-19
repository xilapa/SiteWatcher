using SiteWatcher.Application.Interfaces;
using AutoMapper;
using MediatR;
using SiteWatcher.Application.Metadata;
using SiteWatcher.Application.Notifications;
using SiteWatcher.Application.Validators;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : Validable<RegisterUserCommand>, IRequest<ApplicationResult<string>>
{
    public RegisterUserCommand() : base(new RegisterUserCommandValidator())
    { }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public string? GoogleId { get; set; }
    public string? AuthEmail { get; set; }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApplicationResult<string>>
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(IMapper mapper, IMediator mediator, IUserRepository userRepository, IUnityOfWork uow, ITokenService tokenService)
    {
        _mapper = mapper;
        _mediator = mediator;
        _userRepository = userRepository;
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<ApplicationResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request);
        var appResult = new ApplicationResult<string>();

        try
        {
            _userRepository.Add(user);
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