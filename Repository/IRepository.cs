using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Inmobiliaria.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> SaveAsync();
        Task<bool> ExistsAsync(int id);

        // ✅ NUEVO: paginación + filtrado + includes TODO en SQL
        Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        // ✅ NUEVO: para combos (Select2 / autocompletar) con filtro server-side
        Task<(IEnumerable<T> Items, int Total)> SearchAsync(
            string term,
            int page,
            int pageSize,
            Expression<Func<T, bool>>? extraFilter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Expression<Func<T, string>>[]? searchFields = null);
    }
}