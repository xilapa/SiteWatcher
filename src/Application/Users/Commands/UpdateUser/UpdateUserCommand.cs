using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<CommandResult>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }

    public UpdateUserInput ToInputModel() => new (Name!, Email!, Language, Theme);
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, CommandResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork uow,
        IAuthService authService,
        ISession session)
    {
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _session = session;
    }

    public async Task<CommandResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return CommandResult.FromError(ApplicationErrors.USER_DO_NOT_EXIST);

        user.Update(request.ToInputModel(), _session.Now);
        await _uow.SaveChangesAsync(cancellationToken);

        var newToken = _authService.GenerateLoginToken(user);
        await _authService.WhiteListTokenForCurrentUser(newToken);

        return CommandResult.FromValue(new UpdateUserResult(newToken, !user.EmailConfirmed));
    }
}