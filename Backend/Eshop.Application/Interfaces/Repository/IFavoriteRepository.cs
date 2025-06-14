using Eshop.Core.Entities;

namespace Eshop.Application.Interfaces.Repository
{
    public interface IFavoriteRepository : IGenericRepository<Favorite>
    {
        /*        public IQueryable<Favorite> GetAllFavoritesForCustomer(string customerId);
                public Favorite GetFavProduct(int productID, string customerId);*/
    }

}
