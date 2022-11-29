using System.Linq.Expressions;

namespace SiteWatcher.Common.Repositories;

public interface IRepository<T> where T : class
{
    public T Add(T entity);

    /// <summary>
    /// Se a entidade já estiver sendo trackeada, retorna sem acesso ao banco.
    /// </summary>
    /// <param name="predicate">Predicado para busca de <typeparamref name="T"/></param>
    /// <returns><typeparamref name="T"/> ou null</returns>
    Task<T?> FindAsync(Expression<Func<T,bool>> predicate, CancellationToken cancellationToken);

    /// <summary>
    /// Busca a entidade no banco de dados.
    /// </summary>
    /// <param name="predicate">Predicado para busca de <typeparamref name="T"/></param>
    /// <returns><typeparamref name="T"/> ou null</returns>
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
}