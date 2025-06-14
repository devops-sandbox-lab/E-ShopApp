using Eshop.Core.Entities;

namespace Eshop.Application.Interfaces.Repository
{
    public interface IProductRepository : IGenericRepository<Product>
    {


        Task DeleteImagesAsync(List<int> ids);
        Task<ProductImages> GetImageByID(int id);

        public Task<bool> IsFav(int productId, string UserId);

    }
}
