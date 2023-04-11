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

    public async Task Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetUserByIdAsync(_session.UserId.Value, ct);
        var userDeleted = await _userRepository.DeleteActiveUserAsync(_session.UserId!.Value, ct);
        if (!userDeleted) return;

        var accountDeletedEvent = new AccountDeletedEvent(user.Name, user.Email, user.Language);
        await _mediator.Publish(accountDeletedEvent, CancellationToken.None);
    }
}