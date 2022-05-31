using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : Validable<RegisterUserCommand>, IRequest<ICommandResult<string>>
{
    public RegisterUserCommand() : base(new RegisterUserCommandValidator())
    { }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
    public string? GoogleId { get; set; }
    public string? AuthEmail { get; set; }

    public void GetSessionValues(ISessao sessao)
    {
        AuthEmail = sessao.Email;
        GoogleId = sessao.GoogleId;
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ICommandResult<string>>
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;
    private readonly IAuthService _authService;
    private readonly ISessao _sessao;

    public RegisterUserCommandHandler(IMapper mapper, IMediator mediator, IUserRepository userRepository,
        IUnityOfWork uow, IAuthService authService, ISessao sessao)
    {
        _mapper = mapper;
        _mediator = mediator;
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _sessao = sessao;
    }

    public async Task<ICommandResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        request.GetSessionValues(_sessao);
        var user = _mapper.Map<User>(request);
        var appResult = new CommandResult<string>();

        try
        {
            _userRepository.Add(user);
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch(UniqueViolationException ex)
        {
            return appResult.WithError(ex.Message);
        }

        var token = _authService.GenerateLoginToken(user);
        await _authService.InvalidateCurrentRegisterToken();

        if(!user.EmailConfirmed)
            await _mediator.Publish(_mapper.Map<UserRegisteredNotification>(user), cancellationToken);

        return appResult.WithValue(token);
    }
}