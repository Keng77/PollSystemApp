using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Infrastructure.Common.Persistence
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _dbSet;

        protected RepositoryBase(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _dbSet = _db.Set<T>();
        }

        public virtual IQueryable<T> FindAll(bool trackChanges = false) =>
          trackChanges
            ? _dbSet
            : _dbSet.AsNoTracking();

        public virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
          trackChanges
            ? _dbSet.Where(expression)
            : _dbSet.Where(expression).AsNoTracking();

        public virtual Task<T?> FindAsync(params object[] keyValues) =>
          _dbSet.FindAsync(keyValues).AsTask();

        public virtual ValueTask<T?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default) =>
          _dbSet.FindAsync(keyValues, cancellationToken);

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default) =>
          await FindByCondition(expression, trackChanges)
                 .FirstOrDefaultAsync(cancellationToken);

        public virtual async Task<List<T>> ListAllAsync(bool trackChanges = false, CancellationToken cancellationToken = default) =>
          await FindAll(trackChanges).ToListAsync(cancellationToken);

        public virtual async Task<List<T>> ListAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default) =>
          await FindByCondition(expression, trackChanges).ToListAsync(cancellationToken);

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default) =>
          await _dbSet.AnyAsync(expression, cancellationToken);

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null, CancellationToken cancellationToken = default) =>
          expression == null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(expression, cancellationToken);


        public virtual void Create(T entity) => _db.Set<T>().Add(entity);
        public virtual async Task CreateAsync(T entity, CancellationToken cancellationToken = default) =>
          await _dbSet.AddAsync(entity, cancellationToken);
        public virtual async Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
          await _dbSet.AddRangeAsync(entities, cancellationToken);

        public void Update(T entity) => _dbSet.Update(entity);
        public virtual void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);

        public void Delete(T entity) => _dbSet.Remove(entity);
        public virtual void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
