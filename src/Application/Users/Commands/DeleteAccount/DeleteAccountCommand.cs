using MediatR;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Users.Events;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class DeleteAccountCommand : IRequest
{ }

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IUserDapperRepository _userRepository;
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public DeleteAccountCommandHandler(IUserDapperRepository userRepository, ISession session, IMediator mediator)
    {
        _userRepository = userRepository;
        _session = session;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userDeleted = await _userRepository.DeleteActiveUserAsync(_session.UserId, cancellationToken);
        if (!userDeleted) return Unit.Value;

        var accountDeletedEvent = new AccountDeletedEvent(_session.Name, _session.Email, _session.Language);
        await _mediator.Publish(accountDeletedEvent, CancellationToken.None);

        return Unit.Value;
    }
}