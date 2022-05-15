using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Infra.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SiteWatcherContext context) : base(context) { }

    public override async Task<User> GetAsync(Expression<Func<User, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(nameof(predicate));
        return (await Context.Users.FirstOrDefaultAsync(predicate))!;
    }
}