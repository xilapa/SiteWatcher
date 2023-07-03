using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;

namespace SiteWatcher.Application.Users.Commands.DeactivateAccount;

public class DeactivateAccountCommand : IRequest
{ }

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public DeactivateAccountCommandHandler(ISiteWatcherContext context, ISession session)
    {
        _context = context;
        _session = session;
    }

    public async Task Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await  _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, cancellationToken);
        if (user is null)
            return;

        user.Deactivate(_session.Now);
        await _context.SaveChangesAsync(cancellationToken);
    }
}