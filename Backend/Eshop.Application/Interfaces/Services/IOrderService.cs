using Eshop.Application.DTOs.Cart;
using Eshop.Application.DTOs.Order;
using Eshop.Application.GeneralResponse;
using Eshop.Core.Enums;

namespace Eshop.Application.Interfaces.Services
{
    public interface IOrderService
    {
        public Task<GeneralResponse<IEnumerable<OrderDTO>>> CreateOrderAsync(int cartId, string userId, AdditionalInfoDTO additionalInfo);
        public Task<GeneralResponse<OrderDTO>> BuyNowAsync(int productId, int sizeId, string userId, AdditionalInfoDTO additionalInfo);

        Task<GeneralResponse<OrderDTO>> GetOrderByIdAsync(int orderId);

        public Task<GeneralResponse<OrderDTO>> TempBuyNowAsync(int productId, int sizeId, string userId, AdditionalInfoDTO additionalInfo);
        public Task<GeneralResponse<IEnumerable<OrderDTO>>> TempCreateOrderAsync(int cartId, string userId, AdditionalInfoDTO additionalInfo);
        Task<GeneralResponse<OrderDTO>> UpdateOrderStatusBySellerAsync(int orderId, OrderStatus newStatus);

        Task<GeneralResponse<OrderDTO>> ConfirmOrderReceivedByCustomerAsync(int orderId);

        Task<GeneralResponse<bool>> CancelOrderByCustomerAsync(int orderId);

        public Task<GeneralResponse<IEnumerable<OrderDTO>>> GetSellerOrdersAsync(string sellerId, int page, int pageSize, List<OrderStatus>? statuses = null);
        Task<GeneralResponse<IEnumerable<OrderDTO>>> GetCustomerOrdersAsync(string customerId, int page, int pageSize, OrderStatus? status = null);




    }
}
