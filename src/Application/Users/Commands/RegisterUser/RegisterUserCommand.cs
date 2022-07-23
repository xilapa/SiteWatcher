using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<ICommandResult<RegisterUserResult>>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
    public string? GoogleId { get; set; }
    public string? AuthEmail { get; set; }

    public void GetSessionValues(ISession session)
    {
        AuthEmail = session.Email;
        GoogleId = session.GoogleId;
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ICommandResult<RegisterUserResult>>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public RegisterUserCommandHandler(IMapper mapper, IUserRepository userRepository,
        IUnitOfWork uow, IAuthService authService, ISession session)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _session = session;
    }

    public async Task<ICommandResult<RegisterUserResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        request.GetSessionValues(_session);
        var user = User.FromInputModel(_mapper.Map<RegisterUserInput>(request), _session.Now);
        var appResult = new CommandResult<RegisterUserResult>();

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

        return appResult.WithValue(new RegisterUserResult(token, !user.EmailConfirmed));
    }
}