using System.Linq.Expressions;
using SiteWatcher.Common.Repositories;

namespace SiteWatcher.Infra.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly SiteWatcherContext Context;

    protected Repository(SiteWatcherContext context)
    {
        ArgumentNullException.ThrowIfNull(nameof(context));
        Context = context;
    }

    public abstract Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);

    public T Add(T entity)
    {
        Context.Add(entity);
        return entity;
    }

    public async Task<T?> FindAsync(Expression<Func<T,bool>> predicate, CancellationToken cancellationToken)
    {
        var trackedEntities = Context.ChangeTracker.Entries<T>().Select(e => e.Entity);

        var entity = trackedEntities.FirstOrDefault(predicate.Compile());
        if (entity is not null)
            return entity;

        return await GetAsync(predicate, cancellationToken: cancellationToken);
    }
}