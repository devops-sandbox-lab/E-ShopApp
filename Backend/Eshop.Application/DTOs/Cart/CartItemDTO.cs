namespace Eshop.Application.DTOs.Cart
{
    public class CartItemDTO
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string SampleImage { get; set; }
        public string ProductDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int AllQuantityAvalibale { get; set; }
        public int? SizeId { get; set; }
    }
}
