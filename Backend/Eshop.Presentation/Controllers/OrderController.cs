using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs.Cart;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Enums;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create-order")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> CreateOrder(int cartId, AdditionalInfoDTO additionalInfo)
        {
            var userId = User.FindFirstValue("uid");
            var result = await _orderService.TempCreateOrderAsync(cartId, userId, additionalInfo);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }


        [HttpPost("buy-now")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> buyNow(int productId, [FromBody] AdditionalInfoDTO additionalInfo, int sizeId = 0)
        {
            var userId = User.FindFirstValue("uid");
            var result = await _orderService.TempBuyNowAsync(productId, sizeId, userId, additionalInfo);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }



        [HttpGet("get-order-by-id/{orderId}")]
        [Authorize]
        public async Task<ActionResult> GetOrderById(int orderId)
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }
        [HttpGet("seller-orders")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult> GetSellerOrders(int page = 1, int PageSize = 5, [FromQuery] List<OrderStatus> statuses = null)
        {
            var sellerId = User.FindFirstValue("uid");
            var result = await _orderService.GetSellerOrdersAsync(sellerId, page, PageSize, statuses);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }



        [HttpGet("customer-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> GetCustomerOrders([FromQuery] int page = 1, int PageSize = 5, OrderStatus? status = null)
        {
            var customerId = User.FindFirstValue("uid");
            var result = await _orderService.GetCustomerOrdersAsync(customerId, page, PageSize, status);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }
        public class UpdateOrderStatusRequest
        {
            public int OrderId { get; set; }
            public OrderStatus Status { get; set; }
        }

        [HttpPut("update-order-status")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _orderService.UpdateOrderStatusBySellerAsync(request.OrderId, request.Status);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

        [HttpPut("confirm-order-received")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> ConfirmOrderReceived([FromQuery] int orderId)  // Use [FromQuery] to get orderId from the URL
        {
            var result = await _orderService.ConfirmOrderReceivedByCustomerAsync(orderId);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }


        [HttpDelete("cancel-order")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> CancelOrder([FromQuery] int orderId)
        {
            var result = await _orderService.CancelOrderByCustomerAsync(orderId);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

    }
}