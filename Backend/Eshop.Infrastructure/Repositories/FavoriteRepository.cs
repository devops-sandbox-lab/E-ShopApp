using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        public ApplicationDbContext _context { get; }

        public FavoriteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /*        public IQueryable<Favorite> GetAllFavoritesForCustomer(string customerId)
                {
                    // Using the FindAll method from the generic repository
                    // return FindAllAsync(criteria: f => f.CustomerId == customerId).ToList();


                    return _context.FavoriteService
                  .Where(f => f.CustomerId == customerId);

                }

                public Favorite GetFavProduct(int productID, string customerId)
                {
                    return _context
                       .FavoriteService
                       .FirstOrDefault(f => f.ProductId == productID && f.CustomerId == customerId);
                }*/
    }

}
