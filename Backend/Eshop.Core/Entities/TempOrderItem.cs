namespace Eshop.Core.Entities
{
    public class TempOrderItem
    {
        public int TempOrderItemId { get; set; }
        public int TempOrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int? ProductSizeId { get; set; }
        public decimal? Discount { get; set; }

        public TempOrder TempOrder { get; set; }
        public Product Product { get; set; }
        public ProductSize? ProductSize { get; set; }
    }
}
