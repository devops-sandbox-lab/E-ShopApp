namespace Eshop.Application.DTOs.Product
{
    public class ProductSizeDetailDTO
    {
        public int SizeId { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public string? Name { get; set; }
    }
}
