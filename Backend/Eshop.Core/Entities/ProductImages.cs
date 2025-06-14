namespace Eshop.Core.Entities
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        public int productId { get; set; }
        public Product product { get; set; }
    }
}
