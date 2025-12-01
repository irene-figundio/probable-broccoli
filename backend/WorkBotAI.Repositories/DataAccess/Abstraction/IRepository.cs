using System.Linq.Expressions;

namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<IPagedResult<T>> PageAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null,
                                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                        CancellationToken ct = default);

        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(T entity, CancellationToken ct = default);

        IQueryable<T> Query(); // per query complesse/specifiche
    }

    
}

