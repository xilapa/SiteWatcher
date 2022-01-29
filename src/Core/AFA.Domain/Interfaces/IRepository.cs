using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AFA.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    public IUnityOfWork UoW { get; }
    
    public T Add(T entity);

    /// <summary>
    /// Se a entidade já estiver sendo trackeada, retorna sem acesso ao banco.
    /// </summary>
    /// <param name="predicate">Predicado para busca de <typeparamref name="T"/></param>
    /// <returns><typeparamref name="T"/> ou <typeparamref name="null"/></returns>
    Task<T> FindAsync(Expression<Func<T,bool>> predicate);

    /// <summary>
    /// Busca a entidade no banco de dados.
    /// </summary>
    /// <param name="predicate">Predicado para busca de <typeparamref name="T"/></param>
    /// <returns><typeparamref name="T"/> ou <typeparamref name="null"/></returns>
    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
}