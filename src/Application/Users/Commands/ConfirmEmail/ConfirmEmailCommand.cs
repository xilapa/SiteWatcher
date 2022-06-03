using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<ICommandResult<object>>
{
    public string Token { get; set; }
}

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ICommandResult<object>>
{
    private readonly IAuthService _authservice;
    private readonly IUserRepository _userRepository;
    private readonly ISessao _sessao;
    private readonly IUnityOfWork _uow;

    public ConfirmEmailCommandHandler(IAuthService authservice, IUserRepository userRepository, ISessao sessao, IUnityOfWork uow)
    {
        _authservice = authservice;
        _userRepository = userRepository;
        _sessao = sessao;
        _uow = uow;
    }

    public async Task<ICommandResult<object>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var result = new CommandResult<object>();
        var userId = await _authservice.GetUserIdFromEmailConfirmationToken(request.Token);
        if(userId is null)
            return result.WithError(ApplicationErrors.INVALID_TOKEN);

        var user = await _userRepository.GetAsync(u => u.Id == userId && u.Active && !u.EmailConfirmed, cancellationToken);
        if(user is null)
            return result.WithError(ApplicationErrors.INVALID_TOKEN);

        var success = user!.ConfirmEmail(request.Token, _sessao.Now);
        if(!success)
            result.SetError(ApplicationErrors.INVALID_TOKEN);

        await _uow.SaveChangesAsync(CancellationToken.None);
        return result;
    }
}