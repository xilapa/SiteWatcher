using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendReactivateAccountEmailCommand : IRequest
{
    public UserId UserId { get; set; }
}

public class SendReactivateAccountEmailCommandHandler : IRequestHandler<SendReactivateAccountEmailCommand>
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;

    public SendReactivateAccountEmailCommandHandler(ISiteWatcherContext context, ISession session)
    {
        _context = context;
        _session = session;
    }

    public async Task Handle(SendReactivateAccountEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.Active, cancellationToken);
        if(user is null)
            return;

        user.GenerateUserActivationToken(_session.Now);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}