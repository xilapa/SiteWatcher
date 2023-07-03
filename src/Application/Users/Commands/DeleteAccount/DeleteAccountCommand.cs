using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class DeleteAccountCommand : IRequest
{ }

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public DeleteAccountCommandHandler(ISiteWatcherContext context, ISession session)
    {
        _context = context;
        _session = session;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, ct);
        if (user == null) return;

        _context.Users.Remove(user);

        var email = EmailFactory.AccountDeleted(user, _session.Now);
        _context.Emails.Add(email);

        await _context.SaveChangesAsync(ct);
    }
}