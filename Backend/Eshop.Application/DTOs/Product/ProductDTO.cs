namespace Eshop.Application.DTOs.Product
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string? sellerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public List<string>? productImagesUrl { get; set; }

        public List<ProductSizeDTO> ProductSizes { get; set; }
        public int? StockQuantity { get; set; }

    }
}
