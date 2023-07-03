using Mediator;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Exceptions;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommand : ICommand<RegisterUserResult>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }
    public string GoogleId { get; set; }
    public string AuthEmail { get; set; }

    public RegisterUserInput ToInputModel() => new (Name!, Email!, Language, Theme, GoogleId, AuthEmail);
}

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly ISiteWatcherContext _context;
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public RegisterUserCommandHandler(ISiteWatcherContext context, IAuthService authService,
        ISession session)
    {
        _context = context;
        _authService = authService;
        _session = session;
    }

    public async ValueTask<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.FromInputModel(request.ToInputModel(), _session.Now);

        // TODO: remove this exception, not rely on database for a business rule
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
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