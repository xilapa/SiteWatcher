using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Infra.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SiteWatcherContext context) : base(context) { }

    public override async Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nameof(predicate));
        return (await Context.Users.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken))!;
    }
}