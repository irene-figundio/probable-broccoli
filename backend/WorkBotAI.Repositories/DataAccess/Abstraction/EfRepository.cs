using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WBAI_API.Models.Abstractions;

namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;

        public EfRepository(ApplicationDbContext db) => _db = db;

        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
            => await _db.Set<T>().FindAsync(new[] { id }, ct);

        public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
            => await _db.Set<T>().AsNoTracking().ToListAsync(ct);

        public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _db.Set<T>().AsNoTracking().Where(predicate).ToListAsync(ct);

        public virtual async Task<IPagedResult<T>> PageAsync(int page, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken ct = default)
        {
            var q = _db.Set<T>().AsNoTracking().AsQueryable();
            if (predicate != null) q = q.Where(predicate);
            var total = await q.CountAsync(ct);
            if (orderBy != null) q = orderBy(q);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return new PagedResult<T>(items, page, pageSize, total);
        }
        public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
        { var e = await _db.Set<T>().AddAsync(entity, ct); return e.Entity; }

        public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _db.Set<T>().AddRangeAsync(entities, ct);

        public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
        { _db.Set<T>().Update(entity); return Task.CompletedTask; }

        public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            if (entity is ISoftDeletable)
            {
                // Non rimuovere: lascia che il DbContext lo trasformi in soft delete al SaveChanges
                _db.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                // Fallback: hard delete per entità non soft-deletable
                _db.Set<T>().Remove(entity);
            }
            return Task.CompletedTask;
        }

        public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
    }
}

