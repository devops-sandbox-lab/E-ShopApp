namespace Eshop.Application.DTOs.Cart
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public double TotalAmount { get; set; }

        public int CartItemsCount { get; set; }
        public List<CartItemDTO> Items { get; set; }
    }
}
