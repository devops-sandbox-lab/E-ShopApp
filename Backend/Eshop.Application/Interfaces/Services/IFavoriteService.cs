using Eshop.Application.GeneralResponse;

namespace Eshop.Application.Interfaces.Services
{
    public interface IFavoriteService
    {
        public Task<List<int>> GetAllFavIds(string customerID);
        // public bool ToggleFavorite(int serviceID, >string CustomerID);
        public Task<GeneralResponse<bool>> AddProductToFav(int productID, string CustomerID);

        void RemoveProductFromFav(int productID, string customerId);
    }
}
