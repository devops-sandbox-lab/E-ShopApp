using AutoMapper;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Eshop.Application.Configurations;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;
using Product = Eshop.Core.Entities.Product;

namespace Eshop.Application.Services
{
    public class StripeService
    {
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StripeService(IOptions<StripeSettings> stripeSettings, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _stripeSettings = stripeSettings;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(string customerId)
        {
            var tempOrders = await GetTempOrdersForCustomer(customerId);

            if (!tempOrders.Any())
            {
                throw new Exception("No temporary orders found for the customer.");
            }

            var lineItems = new List<SessionLineItemOptions>();
            decimal totalAmount = 0;

            foreach (var order in tempOrders)
            {
                decimal orderTotal = order.TotalAmount;

                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(orderTotal * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order #{order.TempOrderId} Total",
                        },
                    },
                    Quantity = 1,
                });

                if (order.DeliveryFee > 0)
                {
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(order.DeliveryFee * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{order.TempOrderId} Delivery Fee",
                            },
                        },
                        Quantity = 1,
                    });
                }

                totalAmount += orderTotal + (decimal)order.DeliveryFee;
            }

            var service = new SessionService();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                ClientReferenceId = customerId,
                SuccessUrl = "http://localhost:4200/orderConfirm/{CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:4200/orderCancel/{CHECKOUT_SESSION_ID}",
            };



            var session = await service.CreateAsync(options);

            return session.Id;
        }



        private async Task<List<TempOrder>> GetTempOrdersForCustomer(string customerId)
        {
            var res = await _unitOfWork.GetRepository<TempOrder>()
            .FindAllAsync(
            new[] { "TempOrderItems", "TempOrderItems.Product" },
                to => to.BuyerId == customerId
            );
            return res.ToList();
        }


        public async Task<bool> HandleSuccessfulPayment(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                var customerId = session.ClientReferenceId;
                return await MoveOrdersToRealTable(customerId);
            }

            return false;
        }

        public async Task<bool> ProcessFailedPaymentAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            var customerId = session.ClientReferenceId;

            return await RemoveTempOrders(customerId);
        }


        private async Task<bool> MoveOrdersToRealTable(string customerId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var tempOrders = await GetTempOrdersForCustomer(customerId);
                foreach (var tempOrder in tempOrders)
                {
                    if (tempOrder == null)
                    {
                        throw new Exception("Temp order is null");
                    }

                    var order = _mapper.Map<Order>(tempOrder);

                    await _unitOfWork.GetRepository<Order>().AddAsync(order);

                    foreach (var tempItem in tempOrder.TempOrderItems)
                    {
                        var orderItem = _mapper.Map<OrderItem>(tempItem);
                        orderItem.OrderId = order.OrderId;
                        await _unitOfWork.GetRepository<OrderItem>().AddAsync(orderItem);

                        var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(tempItem.ProductId);
                        if (product != null)
                        {
                            if (tempItem.ProductSizeId.HasValue)
                            {
                                var productSize = _unitOfWork.GetRepository<ProductSize>().Find(ps => ps.SizeId == tempItem.ProductSizeId);
                                if (productSize != null)
                                {
                                    productSize.StockQuantity -= tempItem.Quantity;
                                    await _unitOfWork.GetRepository<ProductSize>().UpdateAsync(productSize);
                                }
                            }
                            else
                            {
                                product.StockQuantity -= tempItem.Quantity;
                                await _unitOfWork.GetRepository<Product>().UpdateAsync(product);
                            }
                        }
                    }
                    await _unitOfWork.GetRepository<TempOrder>().DeleteAsync(tempOrder.TempOrderId);
                }

                //Erse Cart ==>
                var Customercart = _unitOfWork.cartRepository.Find(c => c.UserId == customerId);
                if (Customercart != null)
                {
                    var cartItems = await _unitOfWork.cartItemRepository.FindAllAsync(null, c => c.CartId == Customercart.CartId);
                    _unitOfWork.cartItemRepository.DeleteRange(cartItems.ToList());
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        private async Task<bool> RemoveTempOrders(string customerId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var tempOrders = await GetTempOrdersForCustomer(customerId);
                foreach (var tempOrder in tempOrders)
                {
                    var allItems = await _unitOfWork.GetRepository<TempOrderItem>().GetAllAsync();
                    _unitOfWork.GetRepository<TempOrderItem>().DeleteRange(allItems);
                    await _unitOfWork.GetRepository<TempOrder>().DeleteAsync(tempOrder.TempOrderId);
                }
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}