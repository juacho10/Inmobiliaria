using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Inmobiliaria.Data;
using Inmobiliaria.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Inmobiliaria.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity) => _dbSet.Remove(entity);

        public async Task<bool> SaveAsync() => await _context.SaveChangesAsync() > 0;

        public async Task<bool> ExistsAsync(int id) => await _dbSet.FindAsync(id) != null;

        public async Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            if (filter != null)
                query = query.Where(filter);

            var total = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<T> Items, int Total)> SearchAsync(
            string term,
            int page,
            int pageSize,
            Expression<Func<T, bool>>? extraFilter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Expression<Func<T, string>>[]? searchFields = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (extraFilter != null)
                query = query.Where(extraFilter);

            if (!string.IsNullOrWhiteSpace(term) && searchFields != null && searchFields.Length > 0)
            {
                var lowered = term.ToLower();
                Expression<Func<T, bool>>? orExpr = null;

                foreach (var field in searchFields)
                {
                    var parameter = field.Parameters[0];
                    var prop = field.Body;

                    // Normalizar a string: garantizar ToLower() sobre el body
                    Expression body = prop;
                    if (body.Type != typeof(string))
                    {
                        body = Expression.Call(body,
                            typeof(object).GetMethod("ToString")!);
                    }

                    var toLower = Expression.Call(body,
                        typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);

                    var contains = Expression.Call(toLower,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                        Expression.Constant(lowered));

                    var lambda = Expression.Lambda<Func<T, bool>>(contains, parameter);

                    orExpr = orExpr == null ? lambda : orExpr.Or(lambda);
                }

                if (orExpr != null)
                    query = query.Where(orExpr);
            }

            var total = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}