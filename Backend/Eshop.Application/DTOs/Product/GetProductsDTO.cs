namespace Eshop.Application.DTOs.Product
{
    public class GetProductsDTO
    {
        public int ProductId { get; set; }
        public string? sellerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int StockQuantity { get; set; }
        public double? Rating { get; set; }
        public bool IsFavorites { get; set; }
        public List<string>? productImagesUrl { get; set; }
        public List<GetProductFeaturesDTO> ProductFeatures { get; set; }
        public List<GetReviewsDTO> reviews { get; set; }
        public List<ProductSizeDTO> ProductSizes { get; set; }
    }
}
