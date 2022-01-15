using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFA.Domain.Entities;
using AFA.Domain.Interfaces;
using AFA.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace AFA.Infra.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AFAContext context) : base(context) { }

    public override async Task<User> GetAsync(Expression<Func<User, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(nameof(predicate));
        return await context.Users.FirstOrDefaultAsync(predicate);
    }
}