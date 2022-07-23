using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<ICommandResult<UpdateUserResult>>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ICommandResult<UpdateUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly ISession _session;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork uow,
        IAuthService authService,
        IMapper mapper,
        ISession session)
    {
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _mapper = mapper;
        _session = session;
    }

    public async Task<ICommandResult<UpdateUserResult>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var appResult = new CommandResult<UpdateUserResult>();
        var user = await _userRepository
            .GetAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return appResult.WithError(ApplicationErrors.USER_DO_NOT_EXIST);

        user.Update(_mapper.Map<UpdateUserInput>(request), _session.Now);
        await _uow.SaveChangesAsync(cancellationToken);

        var newToken = _authService.GenerateLoginToken(user);
        await _authService.WhiteListTokenForCurrentUser(newToken);

        return appResult.WithValue(new UpdateUserResult(newToken, !user.EmailConfirmed));
    }
}