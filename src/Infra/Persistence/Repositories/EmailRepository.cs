using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.Repositories;

namespace SiteWatcher.Infra.Persistence.Repositories;

public sealed class EmailRepository : IEmailRepository
{
    private readonly SiteWatcherContext _ctx;

    public EmailRepository(SiteWatcherContext ctx)
    {
        _ctx = ctx;
    }

    public void Add(Email email) => _ctx.Add(email);
}