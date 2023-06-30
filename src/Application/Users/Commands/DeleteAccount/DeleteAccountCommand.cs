using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Users.Events;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class DeleteAccountCommand : IRequest
{ }

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public DeleteAccountCommandHandler(ISiteWatcherContext context, ISession session, IMediator mediator)
    {
        _context = context;
        _session = session;
        _mediator = mediator;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, ct);
        if (user == null) return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);

        var accountDeletedEvent = new AccountDeletedEvent(user.Name, user.Email, user.Language);
        await _mediator.Publish(accountDeletedEvent, CancellationToken.None);
    }
}