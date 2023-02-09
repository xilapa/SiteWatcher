using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<CommandResult>
{
    public string? Token { get; set; }
}

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, CommandResult>
{
    private readonly IAuthService _authservice;
    private readonly IUserRepository _userRepository;
    private readonly ISession _session;
    private readonly IUnitOfWork _uow;

    public ConfirmEmailCommandHandler(IAuthService authservice, IUserRepository userRepository, ISession session, IUnitOfWork uow)
    {
        _authservice = authservice;
        _userRepository = userRepository;
        _session = session;
        _uow = uow;
    }

    public async Task<CommandResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if(request.Token == null)
            return ReturnError();

        var userId = await _authservice.GetUserIdFromConfirmationToken(request.Token);
        if(userId is null)
            return ReturnError();

        var user = await _userRepository.GetAsync(u => u.Id == userId && u.Active && !u.EmailConfirmed, cancellationToken);
        if(user is null)
            return ReturnError();

        var success = user.ConfirmEmail(request.Token, _session.Now);
        if(!success)
            return ReturnError();

        await _uow.SaveChangesAsync(CancellationToken.None);
        return CommandResult.Empty();
    }

    private static CommandResult ReturnError() =>
        CommandResult.FromError(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));
}