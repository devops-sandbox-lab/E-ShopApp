namespace Eshop.Core.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
