using System.Linq.Expressions;

namespace Eshop.Application.Interfaces.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        T GetById(int id);
        Task<T> GetByIdAsync(int id);

        T Find(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null);

        Task<IQueryable<T>> FindAllByOrder(string[] includes = null, Expression<Func<T, bool>> criteria = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);

        IQueryable<T> FindAll(string[] includes = null, Expression<Func<T, bool>> criteria = null);
        Task<IQueryable<T>> FindAllAsync(string[] includes = null, Expression<Func<T, bool>> criteria = null);

        void Add(T entity);
        Task AddAsync(T entity);

        void AddRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        public Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        void DeleteRange(IEnumerable<T> entities);
        public Task DeleteRangeAsync(IEnumerable<int> ids);

        Task<IQueryable<T>> GetAllAsync();
        Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>> criteria);
    }
}
