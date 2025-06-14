using Eshop.Core.Enums;

namespace Eshop.Core.Entities
{
    public class TempOrder
    {
        public int TempOrderId { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public double DeliveryFee { get; set; }


        public string Address { get; set; }
        public string phoneNumber { get; set; }

        public ICollection<TempOrderItem> TempOrderItems { get; set; }
    }
}
