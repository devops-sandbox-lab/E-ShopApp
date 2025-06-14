using Eshop.Core.Enums;

namespace Eshop.Application.DTOs.Order
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string SellerId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public double DeliveryFee { get; set; }


        public string Address { get; set; }
        public string phoneNumber { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}
