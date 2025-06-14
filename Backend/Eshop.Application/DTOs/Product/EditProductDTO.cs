using Microsoft.AspNetCore.Http;

namespace Eshop.Application.DTOs.Product
{
    public class EditProductDTO
    {
        public string? sellerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public List<IFormFile>? productImages { get; set; }
        public List<string>? productImagesUrl { get; set; }
        public List<ProductSizeDTO>? ProductSizes { get; set; }
        public int? StockQuantity { get; set; }

        public bool IsFreeShipping { get; set; } = false;
        public double DeliveryFee { get; set; }

        public decimal? Discount { get; set; }

        public string? productSizesJson { get; set; }


    }
}
