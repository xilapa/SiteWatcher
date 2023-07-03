using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;

namespace SiteWatcher.Application.Users.Commands.SendEmailConfirmation;

public class SendEmailConfirmationCommand : IRequest
{ }

public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly ISession _session;
    private readonly ISiteWatcherContext _context;

    public SendEmailConfirmationCommandHandler(ISession session, ISiteWatcherContext context)
    {
        _session = session;
        _context = context;
    }

    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active && !u.EmailConfirmed, cancellationToken);
        if(user is null) return;

        user.GenerateEmailConfirmationToken(_session.Now);
        await _context.SaveChangesAsync(cancellationToken);
    }
}