using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.Repositories;
using SiteWatcher.Infra.Repositories;

namespace SiteWatcher.Infra.Persistence.Repositories;

public sealed class EmailRepository : Repository<Email>, IEmailRepository
{
    public EmailRepository(SiteWatcherContext context) : base(context)
    { }

    public void Add(Email email) => Context.Add(email);

    public override Task<Email?> GetAsync(Expression<Func<Email, bool>> predicate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}