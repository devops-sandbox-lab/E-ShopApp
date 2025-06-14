using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;
using Eshop.Infrastructure.Repositories;

namespace Eshop.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private readonly UserManager<ApplicationUser> userManager;

        private Dictionary<Type, object> _repositories;

        public ICategoryRepository categoryRepository { get; }
        public IProductRepository productRepository { get; }
        public IFeatureRepository featureRepository { get; }


        public IFavoriteRepository favoriteRepository { get; }
        public ICartRepository cartRepository { get; }
        public ICartItemRepository cartItemRepository { get; }

        public IOrderRepository orderRepository { get; }
        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
            this.userManager = userManager;
            this.categoryRepository = new CategoryRepository(context);
            this.productRepository = new ProductRepository(context);
            this.featureRepository = new FeatureRepository(context);
            this.cartRepository = new CartRepository(context);
            this.cartItemRepository = new CartItemRepository(context);
            this.orderRepository = new OrderRepository(context);
            this.favoriteRepository = new FavoriteRepository(context);
        }


        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IGenericRepository<T>;
            }

            var repository = new GenericRepository<T>(_context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}