using AutoMapper;
using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand : Validable<UpdateUserCommand>, IRequest<ICommandResult<UpdateUserResult>>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ICommandResult<UpdateUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _uow;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly ISessao _sessao;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnityOfWork uow,
        IAuthService authService,
        IMapper mapper,
        ISessao sessao)
    {
        _userRepository = userRepository;
        _uow = uow;
        _authService = authService;
        _mapper = mapper;
        _sessao = sessao;
    }

    public async Task<ICommandResult<UpdateUserResult>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var appResult = new CommandResult<UpdateUserResult>();
        var user = await _userRepository
            .GetAsync(u => u.Id == _sessao.UserId && u.Active, cancellationToken);
        if (user is null)
            return appResult.WithError(ApplicationErrors.USER_DO_NOT_EXIST);

        user.Update(_mapper.Map<UpdateUserInput>(request), _sessao.Now);
        await _uow.SaveChangesAsync(cancellationToken);

        var newToken = _authService.GenerateLoginToken(user);
        await _authService.WhiteListTokenForCurrentUser(newToken);

        return appResult.WithValue(new UpdateUserResult(newToken, !user.EmailConfirmed));
    }
}