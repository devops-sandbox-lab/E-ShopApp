using Eshop.Application.DTOs.Product;
using Eshop.Application.GeneralResponse;
using Eshop.Core.Entities;

namespace Eshop.Application.Interfaces.Services
{
    public interface IProductService
    {

        public Task<GeneralResponse<string>> GetSizeNameByProductSizeId(int productSizeId);
        public Task<GeneralResponse<ProductDTO>> AddProduct(AddProductDTO addProductDTO);

        public Task<GeneralResponse<ProductDTO>> EditProduct(EditProductDTO editProductDTO, int productId);

        public Task<GeneralResponse<List<GetSizeDTO>>> GetAllSizesAsync();


        public Task<GeneralResponse<List<ProductSizeDetailDTO>>> GetProductSizeDetailsAsync(int productId);

        public Task<GeneralResponse<bool>> DeleteSizeAsync(int sizeId, int productId);

        public Task<GeneralResponse<List<AddProductFeaturesDTO>>> AddProductFeatures(List<AddProductFeaturesDTO> addProductServices);
        Task<GeneralResponse<List<EditFeatureDTO>>> EditProductFeatures(List<EditFeatureDTO> productFeatureDTOs);
        Task<GeneralResponse<bool>> DeleteProductFeature(int featureId);
        public Task<GeneralResponse<bool>> DeleteProductImages(List<int> imageIds);

        public Task<GeneralResponse<Product>> GetProductById(int productId, string UserId = null);
        public Task<GeneralResponse<List<GetProductsDTO>>> GetProductsByIds(List<int> productIds, int page = 1, int pageSize = 10);

        public Task<GeneralResponse<List<GetProductsDTO>>> GetAllSellerProducts(string sellerId, int page = 1, int pageSize = 10);

        public Task<GeneralResponse<List<Product>>> GetProductsByMainCategory(
    int categoryId,
    int page = 1,
    int pageSize = 10,
    string searchTerm = "",
    decimal? minPrice = null,
    decimal? maxPrice = null,
    string sortOption = "",
             int sortsubcategoryId = 0);

        public Task<GeneralResponse<List<Product>>> GetProductsBySubCategory(
 int subCategoryId,
 int page = 1,
 int pageSize = 10,
 string searchTerm = "",
 decimal? minPrice = null,
 decimal? maxPrice = null,
 string sortOption = ""
         );

        public Task<GeneralResponse<List<Product>>> GetSearchProduct(
int page = 1,
int pageSize = 10,
string searchTerm = "",
decimal? minPrice = null,
decimal? maxPrice = null,
string sortOption = ""
        );
        public Task<GeneralResponse<bool>> AddBulkProducts(List<AddProductDTO> addProductDTOs);

        Task<GeneralResponse<List<int>>> GetProductImageIds(int productId);
        Task<GeneralResponse<bool>> DeleteProduct(int productId);

        Task<GeneralResponse<List<Product>>> GetRecommendedProductsAsync();
        Task<GeneralResponse<List<Product>>> GetDealProductsAsync();

        Task<GeneralResponse<List<Product>>> GetBestSellerProductsAsync();
        Task<GeneralResponse<List<Product>>> GetRandomProductsAsync();

        Task<GeneralResponse<List<Product>>> GetMoreFromSellerProductsAsync(string sellerId);
        Task<GeneralResponse<List<Product>>> GetMoreFromCategoryProductsAsync(int CategoryId);

    }
}
