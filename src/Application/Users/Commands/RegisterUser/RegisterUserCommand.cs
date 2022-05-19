using SiteWatcher.Application.Interfaces;
using AutoMapper;
using MediatR;
using SiteWatcher.Application.Metadata;
using SiteWatcher.Application.Validators;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

//Todo criar interface ICommand que herda de Validable e IRequest
public class RegisterUserCommand : Validable<RegisterUserCommand>, IRequest<ApplicationResult<string>>
{
    public RegisterUserCommand() : base(new RegisterUserCommandValidator())
    { }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public string? GoogleId { get; set; }
    public string? AuthEmail { get; set; }

    public void GetSessionValues(ISessao sessao)
    {
        AuthEmail = sessao.Email;
        GoogleId = sessao.GoogleId;
    }
}

//TODO: fazer o mesmo para o requesthandler, criar uma interface que vai herdar de IRequestHandler
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApplicationResult<string>>
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly ISessao _sessao;

    public RegisterUserCommandHandler(IMapper mapper, IMediator mediator, IUserRepository userRepository,
        IUnityOfWork uow, ITokenService tokenService, ISessao sessao)
    {
        _mapper = mapper;
        _mediator = mediator;
        _userRepository = userRepository;
        _uow = uow;
        _tokenService = tokenService;
        _sessao = sessao;
    }

    public async Task<ApplicationResult<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        request.GetSessionValues(_sessao);
        var user = _mapper.Map<User>(request);
        var appResult = new ApplicationResult<string>();

        try
        {
            _userRepository.Add(user);
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch(UniqueViolationException ex)
        {
            // renomear para with error, ou adicionar outro método
            return appResult.AddError(ex.Message);
        }

        var token = _tokenService.GenerateLoginToken(user);
        await _tokenService.InvalidateToken(_sessao.AuthTokenPayload!, ETokenPurpose.Register);

        if(!user.EmailConfirmed)
            await _mediator.Publish(_mapper.Map<UserRegisteredNotification>(user), cancellationToken);

        return appResult.SetValue(token);
    }
}