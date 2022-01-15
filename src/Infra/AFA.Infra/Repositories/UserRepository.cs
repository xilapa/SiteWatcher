using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFA.Domain.Entities;
using AFA.Domain.Interfaces;

namespace AFA.Infra.Repositories;

public class UserRepository : IUserRepository
{
    public IUnityOfWork UoW => throw new NotImplementedException();

    public User Add(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<User> FindAsync(Expression<Func<User, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetAsync(Expression<Func<User, bool>> predicate)
    {
        throw new NotImplementedException();
    }
}