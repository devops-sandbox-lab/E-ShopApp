using Application.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Eshop.Application.DTOs.Cart;
using Eshop.Application.DTOs.Order;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Helpers;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;
using Eshop.Core.Enums;
using Order = Eshop.Core.Entities.Order;

namespace Eshop.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }

        public async Task<GeneralResponse<IEnumerable<OrderDTO>>> CreateOrderAsync(int cartId, string userId, AdditionalInfoDTO additionalInfo)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _unitOfWork.cartRepository.FindAsync(
                    c => c.UserId == userId,
                    new string[] { "CartItems.Product", "CartItems.Product.ProductSizes", "CartItems.ProductSize.Size" }
                );

                if (cart == null || !cart.CartItems.Any())
                {
                    return new GeneralResponse<IEnumerable<OrderDTO>>
                    {
                        Succeeded = false,
                        Message = "Cart is empty or not found."
                    };
                }

                var orders = new List<Order>();
                var groupedItems = cart.CartItems.GroupBy(ci => ci.Product.sellerId);

                foreach (var item in cart.CartItems)
                {
                    var product = await _unitOfWork.productRepository.FindAsync(p => p.ProductId == item.ProductId);
                    product.PurchaseNumber = (product.PurchaseNumber ?? 0) + 1;
                    await _unitOfWork.productRepository.UpdateAsync(product);
                }

                foreach (var group in groupedItems)
                {
                    var order = new Order
                    {
                        BuyerId = userId,
                        SellerId = group.Key,
                        OrderDate = DateTime.Now,
                        Status = OrderStatus.Pending,
                        OrderItems = new List<OrderItem>(),
                        TotalAmount = 0m,
                        DeliveryFee = 0.0
                    };
                    order.Address = additionalInfo.Address;
                    order.phoneNumber = additionalInfo.PhoneNumber;
                    await _unitOfWork.orderRepository.AddAsync(order);

                    double highestDeliveryFee = 0.0;

                    foreach (var item in group)
                    {
                        decimal price = ProductSizeHelper.RequiresSizes(item.Product.CategoryId) ?
                            (decimal)item.ProductSize.Price :
                            (decimal)item.Product.Price;

                        var orderItem = new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = price,
                            OrderId = order.OrderId,
                            ProductSizeId = ProductSizeHelper.RequiresSizes(item.Product.CategoryId) ? item.ProductSizeId : null,
                            Discount = item.Product.Discount,
                        };

                        order.OrderItems.Add(orderItem);

                        order.TotalAmount += (orderItem.Discount != null
            ? (decimal)(orderItem.Price * (1 - (orderItem.Discount / 100))) * orderItem.Quantity
            : orderItem.Price * orderItem.Quantity);


                        if (!item.Product.IsFreeShipping)
                        {
                            highestDeliveryFee = Math.Max(highestDeliveryFee, item.Product.DeliveryFee);
                        }

                        await _unitOfWork.GetRepository<OrderItem>().AddAsync(orderItem);

                        if (ProductSizeHelper.RequiresSizes(item.Product.CategoryId) && item.ProductSize != null)
                        {
                            item.ProductSize.StockQuantity -= item.Quantity;
                            await _unitOfWork.GetRepository<ProductSize>().UpdateAsync(item.ProductSize);
                        }
                        else
                        {
                            item.Product.StockQuantity -= item.Quantity;
                            await _unitOfWork.productRepository.UpdateAsync(item.Product);
                        }
                    }

                    if (highestDeliveryFee > 0)
                    {
                        order.DeliveryFee = highestDeliveryFee;
                    }

                    await _unitOfWork.SaveChangesAsync();

                    orders.Add(order);
                }

                _unitOfWork.cartItemRepository.DeleteRange(cart.CartItems);
                await _unitOfWork.SaveChangesAsync();

                ////////////////////////////////////////////////
                // HERE ADD THE PAYMENT
                ///////////////////////////////////////////////
                ///
       /*         var paymentResponse = await ProcessPaymentAsync(orders);

                if (!paymentResponse.IsSuccess)
                {
                    // If payment fails, rollback the transaction
                    await transaction.RollbackAsync();
                    return new GeneralResponse<IEnumerable<OrderDTO>>
                    {
                        Succeeded = false,
                        Message = "Payment failed. Please try again.",
                        Errors = paymentResponse.Errors
                    };
                }
*/
                await transaction.CommitAsync();

                var orderDtos = _mapper.Map<List<OrderDTO>>(orders);

                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = true,
                    Data = orderDtos,
                    Message = "Orders created successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred while creating the orders.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<GeneralResponse<OrderDTO>> BuyNowAsync(int productId, int sizeId, string userId, AdditionalInfoDTO additionalInfo)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var product = await _unitOfWork.productRepository.FindAsync(p => p.ProductId == productId, new string[] { "ProductSizes" });

                if (product == null)
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Product not found."
                    };
                }

                var productSize = product.ProductSizes.FirstOrDefault(ps => ps.SizeId == sizeId);
                if (ProductSizeHelper.RequiresSizes(product.CategoryId) && (productSize == null || productSize.StockQuantity <= 0))
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Selected size is out of stock."
                    };
                }

                product.PurchaseNumber = (product.PurchaseNumber ?? 0) + 1;
                await _unitOfWork.productRepository.UpdateAsync(product);

                var order = new Order
                {
                    BuyerId = userId,
                    SellerId = product.sellerId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>(),
                    TotalAmount = 0m,
                    DeliveryFee = product.IsFreeShipping ? 0 : product.DeliveryFee,
                };
                order.Address = additionalInfo.Address;
                order.phoneNumber = additionalInfo.PhoneNumber;
                await _unitOfWork.orderRepository.AddAsync(order);

                decimal price = ProductSizeHelper.RequiresSizes(product.CategoryId) ? (decimal)productSize.Price : (decimal)product.Price;

                var orderItem = new OrderItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = price,
                    OrderId = order.OrderId,
                    ProductSizeId = ProductSizeHelper.RequiresSizes(product.CategoryId) ? sizeId : null,
                    Discount = product.Discount,
                };

                order.OrderItems.Add(orderItem);

                order.TotalAmount += (orderItem.Discount != null
                    ? (decimal)(orderItem.Price * (1 - (orderItem.Discount / 100))) * orderItem.Quantity
                    : orderItem.Price * orderItem.Quantity);

                await _unitOfWork.GetRepository<OrderItem>().AddAsync(orderItem);

                if (ProductSizeHelper.RequiresSizes(product.CategoryId) && productSize != null)
                {
                    productSize.StockQuantity -= 1;
                    await _unitOfWork.GetRepository<ProductSize>().UpdateAsync(productSize);
                }
                else
                {
                    product.StockQuantity -= 1;
                    await _unitOfWork.productRepository.UpdateAsync(product);
                }

                // HERE ADD THE PAYMENT
                // For example, you could call a payment service method that processes the payment



                //EX => 

                /*      var paymentResponse = await ProcessPaymentAsync(order);

                      if (!paymentResponse.IsSuccess)
                      {
                          // If payment fails, rollback the transaction
                          await transaction.RollbackAsync();
                          return new GeneralResponse<OrderDTO>
                          {
                              Succeeded = false,
                              Message = "Payment failed. Please try again.",
                              Errors = paymentResponse.Errors
                          };
                      }*/

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);

                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = true,
                    Data = orderDto,
                    Message = "Order created successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while processing the Buy Now order.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        //This two method if you want to use the temp table to save order and then Purchase money then move the order form the temp table to the real table 

        public async Task<GeneralResponse<OrderDTO>> TempBuyNowAsync(int productId, int sizeId, string userId, AdditionalInfoDTO additionalInfo)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var product = await _unitOfWork.productRepository.FindAsync(p => p.ProductId == productId, new string[] { "ProductSizes" });

                if (product == null)
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Product not found."
                    };
                }

                var productSize = product.ProductSizes.FirstOrDefault(ps => ps.SizeId == sizeId);
                if (ProductSizeHelper.RequiresSizes(product.CategoryId) && (productSize == null || productSize.StockQuantity <= 0))
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Selected size is out of stock."
                    };
                }


                var order = new TempOrder
                {
                    BuyerId = userId,
                    SellerId = product.sellerId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    TempOrderItems = new List<TempOrderItem>(),
                    TotalAmount = 0m,
                    DeliveryFee = product.IsFreeShipping ? 0 : product.DeliveryFee,
                };
                order.Address = additionalInfo.Address;
                order.phoneNumber = additionalInfo.PhoneNumber;
                await _unitOfWork.GetRepository<TempOrder>().AddAsync(order);
                decimal price = ProductSizeHelper.RequiresSizes(product.CategoryId) ? (decimal)productSize.Price : (decimal)product.Price;


                var orderItem = new TempOrderItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = price,
                    TempOrderId = order.TempOrderId,
                    ProductSizeId = ProductSizeHelper.RequiresSizes(product.CategoryId) ? sizeId : null,
                    Discount = product.Discount,
                };

                order.TempOrderItems.Add(orderItem);

                order.TotalAmount += (orderItem.Discount != null
                    ? (decimal)(orderItem.Price * (1 - (orderItem.Discount / 100))) * orderItem.Quantity
                    : orderItem.Price * orderItem.Quantity);

                await _unitOfWork.GetRepository<TempOrderItem>().AddAsync(orderItem);


                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);

                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = true,
                    Data = orderDto,
                    Message = "Order created successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while processing the Buy Now order.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<IEnumerable<OrderDTO>>> TempCreateOrderAsync(int cartId, string userId, AdditionalInfoDTO additionalInfo)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _unitOfWork.GetRepository<Cart>()
                            .FindAsync(c => c.CartId == cartId && c.UserId == userId, new[] { "CartItems", "CartItems.Product", "CartItems.Product.ProductSizes" });

                if (cart == null || !cart.CartItems.Any())
                {
                    return new GeneralResponse<IEnumerable<OrderDTO>>
                    {
                        Succeeded = false,
                        Message = "Cart not found or is empty."
                    };
                }

                var tempOrders = new List<TempOrder>();
                var orderDtos = new List<OrderDTO>();

                var cartItemsBySeller = cart.CartItems.GroupBy(item => item.Product.sellerId);

                foreach (var sellerGroup in cartItemsBySeller)
                {
                    var tempOrder = new TempOrder
                    {
                        BuyerId = userId,
                        SellerId = sellerGroup.Key,
                        OrderDate = DateTime.Now,
                        Status = OrderStatus.Pending,
                        TempOrderItems = new List<TempOrderItem>(),
                        TotalAmount = 0m,
                        DeliveryFee = 0.0
                    };

                    tempOrder.Address = additionalInfo.Address;
                    tempOrder.phoneNumber = additionalInfo.PhoneNumber;

                    await _unitOfWork.GetRepository<TempOrder>().AddAsync(tempOrder);

                    decimal? maxDeliveryFee = null;

                    foreach (var cartItem in sellerGroup)
                    {
                        var productSize = cartItem.Product.ProductSizes.FirstOrDefault(ps => ps.SizeId == cartItem.ProductSizeId);
                        if (ProductSizeHelper.RequiresSizes(cartItem.Product.CategoryId) && (productSize == null || productSize.StockQuantity <= 0))
                        {
                            return new GeneralResponse<IEnumerable<OrderDTO>>
                            {
                                Succeeded = false,
                                Message = "Some items in your cart are out of stock."
                            };
                        }

                        decimal price = ProductSizeHelper.RequiresSizes(cartItem.Product.CategoryId) ? (decimal)productSize.Price : (decimal)cartItem.Product.Price;

                        var tempOrderItem = new TempOrderItem
                        {
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = price,
                            TempOrderId = tempOrder.TempOrderId,
                            ProductSizeId = ProductSizeHelper.RequiresSizes(cartItem.Product.CategoryId) ? cartItem.ProductSizeId : null,
                            Discount = cartItem.Product.Discount

                        };

                        tempOrder.TempOrderItems.Add(tempOrderItem);
                        await _unitOfWork.GetRepository<TempOrderItem>().AddAsync(tempOrderItem);


                        tempOrder.TotalAmount += (tempOrderItem.Discount != null
    ? (decimal)(tempOrderItem.Price * (1 - (tempOrderItem.Discount / 100))) * tempOrderItem.Quantity
    : tempOrderItem.Price * tempOrderItem.Quantity);


                        if (!cartItem.Product.IsFreeShipping && cartItem.Product.DeliveryFee > 0)
                        {
                            if (maxDeliveryFee == null || cartItem.Product.DeliveryFee > (double)maxDeliveryFee)
                            {
                                maxDeliveryFee = (decimal)cartItem.Product.DeliveryFee;
                            }
                        }
                    }
                    if (maxDeliveryFee > 0)
                    {
                        tempOrder.DeliveryFee = (double)maxDeliveryFee;
                    }




                    await _unitOfWork.SaveChangesAsync();

                    var orderDto = _mapper.Map<OrderDTO>(tempOrder);
                    orderDtos.Add(orderDto);

                    tempOrders.Add(tempOrder);
                }

                await transaction.CommitAsync();

                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = true,
                    Data = orderDtos,
                    Message = "Orders created successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred while creating the orders.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<GeneralResponse<OrderDTO>> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.orderRepository.FindAsync(o => o.OrderId == orderId, new string[] { "OrderItems", "OrderItems.Product", });
                if (order == null)
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Order not found."
                    };
                }

                var orderDto = _mapper.Map<OrderDTO>(order);
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = true,
                    Data = orderDto
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving the order.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<OrderDTO>> UpdateOrderStatusBySellerAsync(int orderId, OrderStatus newStatus)
        {
            try
            {
                var order = await _unitOfWork.orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Order not found."
                    };
                }

                order.Status = newStatus;
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = true,
                    Data = orderDto,
                    Message = "Order status updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while updating the order status.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<OrderDTO>> ConfirmOrderReceivedByCustomerAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new GeneralResponse<OrderDTO>
                    {
                        Succeeded = false,
                        Message = "Order not found."
                    };
                }

                order.Status = OrderStatus.Delivered;
                await _unitOfWork.SaveChangesAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = true,
                    Data = orderDto,
                    Message = "Order confirmed as received."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<OrderDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while confirming the order.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<bool>> CancelOrderByCustomerAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = "Order not found."
                    };
                }

                if (order.Status != OrderStatus.Pending)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = "Only pending orders can be cancelled."
                    };
                }

                order.Status = OrderStatus.Cancelled;
                await _unitOfWork.SaveChangesAsync();

                return new GeneralResponse<bool>
                {
                    Succeeded = true,
                    Data = true,
                    Message = "Order cancelled successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "An error occurred while cancelling the order.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<IEnumerable<OrderDTO>>> GetSellerOrdersAsync(string sellerId, int page, int pageSize, List<OrderStatus>? statuses = null)
        {
            try
            {
                IQueryable<Order> orders;

                if (statuses == null || statuses.Count == 0)
                {
                    orders = await _unitOfWork.orderRepository.FindAllByOrder(
                        new string[] { "OrderItems", "OrderItems.Product" },
                        o => o.SellerId == sellerId,
                        orderBy: o => o.OrderByDescending(i => i.OrderId));
                }
                else
                {
                    orders = await _unitOfWork.orderRepository.FindAllByOrder(
                        new string[] { "OrderItems", "OrderItems.Product" },
                        o => statuses.Contains(o.Status) && o.SellerId == sellerId,
                        orderBy: o => o.OrderByDescending(i => i.OrderId));
                }

                var paginatedList = PaginationHelper.Paginate(orders, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(paginatedList.Items);

                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = true,
                    Data = orderDtos,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving user orders.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<IEnumerable<OrderDTO>>> GetCustomerOrdersAsync(string customerId, int page, int pageSize, OrderStatus? status = null)
        {
            try
            {
                IQueryable<Order> orders;
                if (status == null)
                {

                    orders = await _unitOfWork.orderRepository.FindAllByOrder(new string[] { "OrderItems", "OrderItems.Product" }, o => o.BuyerId == customerId, orderBy: o => o.OrderByDescending(i => i.OrderId));
                }
                else
                {
                    orders = await _unitOfWork.orderRepository.FindAllByOrder(new string[] { "OrderItems", "OrderItems.Product" }, o => o.Status == status && o.BuyerId == customerId, orderBy: o => o.OrderByDescending(i => i.OrderId));
                }

                var paginatedList = PaginationHelper.Paginate(orders, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(paginatedList.Items);
                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = true,
                    Data = orderDtos,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<IEnumerable<OrderDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving user orders.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}