namespace Eshop.Core.Entities
{
    public class Reviews
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public string customerName { get; set; }
        public string customerImage { get; set; }
        public string reviewImageUrl { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
