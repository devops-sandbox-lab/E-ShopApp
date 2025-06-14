namespace Eshop.Application.DTOs.Cart
{
    public class AddToCartDTO
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public int SizeId { get; set; }
    }
}
