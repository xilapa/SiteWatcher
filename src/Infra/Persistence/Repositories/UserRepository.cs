using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Infra.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SiteWatcherContext context) : base(context) { }

    public override async Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nameof(predicate));
        return (await Context.Users.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken))!;
    }

    public async Task<IEnumerable<User>> GetUserWithAlertsAsync(IEnumerable<Frequencies> frequencies, int take, DateTime? lastCreatedAt, CancellationToken ct) =>
         await Context
            .Users
            .OrderBy(_ => _.CreatedAt)
            .Where(u =>
                u.Active
                && u.EmailConfirmed
                && (!lastCreatedAt.HasValue || (lastCreatedAt.HasValue! && u.CreatedAt > lastCreatedAt)))
            .Include(u => u.Alerts.Where(_ => frequencies.Contains(_.Frequency)))
            .ThenInclude(a => a.Rule)
            .Take(take)
            .ToArrayAsync(ct);
}