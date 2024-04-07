using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Core.Interfaces;

public interface IRepository <T>
{
    public IQueryable<T> All { get; }
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>,
        IIncludableQueryable<T, object>>? include = null);

    public IQueryable<T> GetAll(
        List<Expression<Func<T, bool>>>? filters = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );
    
    Task<List<T>> GetAllAsync(
        List<Expression<Func<T, bool>>>? filters = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    void Add(T entity);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> range);

    void Update(T entity);
}