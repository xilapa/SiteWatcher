using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Exceptions;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
    public string? GoogleId { get; set; }
    public string? AuthEmail { get; set; }

    public RegisterUserInput ToInputModel(ISession session) =>
        new (Name!, Email!, Language, Theme, session.GoogleId!, session.Email!);
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork uow, IAuthService authService,
        ISession session)
    {
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _session = session;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.FromInputModel(request.ToInputModel(_session), _session.Now);

        try
        {
            _userRepository.Add(user);
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueViolationException)
        {
            return RegisterUserResult.AlreadyExists();
        }

        var token = _authService.GenerateLoginToken(user);
        await _authService.InvalidateCurrentRegisterToken();

        return RegisterUserResult.Registered(token, !user.EmailConfirmed);
    }
}