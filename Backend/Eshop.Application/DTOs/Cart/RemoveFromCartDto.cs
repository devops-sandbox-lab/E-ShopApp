namespace Eshop.Application.DTOs.Cart
{
    public class RemoveFromCartDto
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public int ProductSizeId { get; set; }
    }
}
