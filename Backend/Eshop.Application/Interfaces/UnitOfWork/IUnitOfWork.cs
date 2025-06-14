using Microsoft.EntityFrameworkCore.Storage;
using Eshop.Application.Interfaces.Repository;

namespace Eshop.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        public ICategoryRepository categoryRepository { get; }
        public IProductRepository productRepository { get; }
        public IFeatureRepository featureRepository { get; }

        public IFavoriteRepository favoriteRepository { get; }

        public ICartItemRepository cartItemRepository { get; }
        public ICartRepository cartRepository { get; }

        public IOrderRepository orderRepository { get; }
        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
