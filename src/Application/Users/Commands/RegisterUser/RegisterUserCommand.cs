using MediatR;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Exceptions;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }

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

        // TODO: remove this exception, not rely on database for a business rule
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
        await _authService.InvalidateCurrentRegisterToken(_session);

        return RegisterUserResult.Registered(token, !user.EmailConfirmed);
    }
}