using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFA.Domain.Interfaces;
using AFA.Infra.Data;

namespace AFA.Infra.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected AFAContext context;
    
    public Repository(AFAContext context)
    {
        ArgumentNullException.ThrowIfNull(nameof(context));
        this.context = context;
    }

    public IUnityOfWork UoW => context;

    public abstract Task<T> GetAsync(Expression<Func<T, bool>> predicate);

    public T Add(T entity)
    {
        context.Add(entity);
        return entity;
    }

    public async Task<T> FindAsync(Expression<Func<T,bool>> predicate)
    {
        var trackedEntities = context.ChangeTracker.Entries<T>().Select(e => e.Entity);

        var entity = trackedEntities.FirstOrDefault(predicate.Compile());
        if (entity is not null)
            return entity;
  
        return await this.GetAsync(predicate);
    }
}