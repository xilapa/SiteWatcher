using System.Linq.Expressions;
using SiteWatcher.Domain.Interfaces;

namespace SiteWatcher.Data.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly SiteWatcherContext Context;

    protected Repository(SiteWatcherContext context)
    {
        ArgumentNullException.ThrowIfNull(nameof(context));
        Context = context;
    }

    public IUnityOfWork UoW => Context;

    public abstract Task<T> GetAsync(Expression<Func<T, bool>> predicate);

    T IRepository<T>.Add(T entity)
    {
        Context.Add(entity);
        return entity;
    }

    public async Task<T> FindAsync(Expression<Func<T,bool>> predicate)
    {
        var trackedEntities = Context.ChangeTracker.Entries<T>().Select(e => e.Entity);

        var entity = trackedEntities.FirstOrDefault(predicate.Compile());
        if (entity is not null)
            return entity;

        return await this.GetAsync(predicate);
    }
}