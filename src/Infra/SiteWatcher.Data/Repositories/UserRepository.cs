using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace SiteWatcher.Infra.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SiteWatcherContext context) : base(context) { }

    public override async Task<User> GetAsync(Expression<Func<User, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(nameof(predicate));
        return await context.Users.FirstOrDefaultAsync(predicate);
    }
}