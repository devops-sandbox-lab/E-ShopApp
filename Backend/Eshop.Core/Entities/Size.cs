namespace Eshop.Core.Entities
{
    public class Size
    {
        public int SizeId { get; set; }
        public string Name { get; set; }


        public ICollection<ProductSize> ProductSizes { get; set; }
    }
}
