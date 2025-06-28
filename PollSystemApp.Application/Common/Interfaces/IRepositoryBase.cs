using System.Linq.Expressions;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll(bool trackChanges = false);

        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);

        Task<T?> FindAsync(params object[] keyValues);

        ValueTask<T?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default);

        Task<List<T>> ListAllAsync(bool trackChanges = false, CancellationToken cancellationToken = default);

        Task<List<T>> ListAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);

        Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default);

        void Create(T entity);

        Task CreateAsync(T entity, CancellationToken cancellationToken = default);

        Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);

        void DeleteRange(IEnumerable<T> entities);

        Task SaveAsync();
    }

}
