namespace Eshop.Core.Entities
{
    public class Favorite
    {
        public int ID { get; set; }
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
