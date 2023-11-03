using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Application.Users.Commands.SendEmailConfirmation;

public class SendEmailConfirmationCommandHandler : IApplicationHandler
{
    private readonly ISession _session;
    private readonly ISiteWatcherContext _context;
    private readonly IPublisher _publisher;

    public SendEmailConfirmationCommandHandler(ISession session, ISiteWatcherContext context, IPublisher publisher)
    {
        _session = session;
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active && !u.EmailConfirmed, ct);
        if(user is null) return;

        var emailConfirmationTokenGeneratedMessage = user.GenerateEmailConfirmationToken(_session.Now);
        if (emailConfirmationTokenGeneratedMessage != null)
            await _publisher.PublishAsync(emailConfirmationTokenGeneratedMessage, ct);
        await _context.SaveChangesAsync(ct);
    }
}