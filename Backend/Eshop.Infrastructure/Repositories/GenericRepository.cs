using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Eshop.Application.Interfaces.Repository;

namespace Eshop.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }


        public T Find(Expression<Func<T, bool>> criteria, string[] includes = null)
        {

            IQueryable<T> query = _dbSet.Where(criteria);
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query.FirstOrDefault();

        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _dbSet.Where(criteria);
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            var result = await query.FirstOrDefaultAsync();
            return result;
        }

        public IQueryable<T> FindAll(string[] includes = null, Expression<Func<T, bool>> criteria = null)
        {
            IQueryable<T> query = _dbSet;
            if (criteria != null)
            {
                query = query.Where(criteria);
            }
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }

        public async Task<IQueryable<T>> FindAllAsync(string[] includes = null, Expression<Func<T, bool>> criteria = null)
        {
            IQueryable<T> query = _dbSet;
            if (criteria != null)
            {
                query = query.Where(criteria);
            }
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            var result = await query.ToListAsync();
            return result.AsQueryable();
        }


        public async Task<IQueryable<T>> FindAllByOrder(string[] includes = null, Expression<Func<T, bool>> criteria = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (criteria != null)
            {
                query = query.Where(criteria);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query;
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }


        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _dbSet.Update(entity);
        }


        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }


        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }
        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var entities = await _dbSet.Where(e => ids.Contains((int)typeof(T).GetProperty("Id").GetValue(e))).ToListAsync();
            _dbSet.RemoveRange(entities);
        }
        // GetAllAsync method implementation
        public async Task<IQueryable<T>> GetAllAsync()
        {
            var result = await _dbSet.ToListAsync();
            return result.AsQueryable();
        }
        public async Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>> criteria)
        {
            var result = await _dbSet.Where(criteria).ToListAsync();
            return result.AsQueryable();
        }
    }
}
