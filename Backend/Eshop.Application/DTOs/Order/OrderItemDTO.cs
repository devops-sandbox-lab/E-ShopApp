namespace Eshop.Application.DTOs.Order
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string SellerId { get; set; }
        public int? ProductSizeId { get; set; }
    }
}
