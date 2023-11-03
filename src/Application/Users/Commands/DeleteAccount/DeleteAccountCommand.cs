using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Common.Command;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Application.Users.Commands.DeleteUser;

public class DeleteAccountCommandHandler : IApplicationHandler
{
    private readonly ISiteWatcherContext _context;
    private readonly ISession _session;
    private readonly IPublisher _publisher;

    public DeleteAccountCommandHandler(ISiteWatcherContext context, ISession session, IPublisher publisher)
    {
        _context = context;
        _session = session;
        _publisher = publisher;
    }

    public async Task Handle(CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _session.UserId && u.Active, ct);
        if (user == null) return;

        _context.Users.Remove(user);

        var (email, emailCreatedMessage) = EmailFactory.AccountDeleted(user, _session.Now);
        _context.Emails.Add(email);
        await _publisher.PublishAsync(emailCreatedMessage, ct);

        await _context.SaveChangesAsync(ct);
    }
}