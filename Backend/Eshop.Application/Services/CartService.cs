using AutoMapper;
using Eshop.Application.DTOs.Cart;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Helpers;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;

namespace Eshop.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<GeneralResponse<CartDTO>> GetCartByUserIdAsync(string userId, int page, int pageSize)
        {
            try
            {
                var cart = await unitOfWork.cartRepository.FindAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    await unitOfWork.cartRepository.AddAsync(cart);
                    await unitOfWork.SaveChangesAsync();
                }

                var items = await unitOfWork.cartItemRepository.FindAllByOrder(
                    new string[] { "Product", "Product.productImages", "ProductSize.Size" },
                    item => item.CartId == cart.CartId,
                    orderBy: c => c.OrderByDescending(i => i.CartItemId)
                );




                /*var paginatedList = PaginationHelper.Paginate(items, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);
*/
                var cartDto = mapper.Map<CartDTO>(cart);

                cartDto.Items = mapper.Map<List<CartItemDTO>>(items.ToList());

                foreach (var item in cartDto.Items)
                {
                    cartDto.TotalAmount += (double)item.Price * item.Quantity;
                }
                cartDto.CartItemsCount = cartDto.Items.Count;
                return new GeneralResponse<CartDTO>
                {
                    Succeeded = true,
                    Data = cartDto,
                    Message = "Cart retrieved successfully.",
                    //PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<CartDTO>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving the cart.",
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                };
            }
        }


        public async Task<GeneralResponse<bool>> AddToCartAsync(AddToCartDTO addToCartDto, string userId)
        {
            try
            {
                var product = await unitOfWork.productRepository.FindAsync(p => p.ProductId == addToCartDto.ProductId);

                if (product == null)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = "Product not found."
                    };
                }

                if (ProductSizeHelper.RequiresSizes(product.CategoryId))
                {
                    return await HandleAddToCartWithSizeAsync(addToCartDto, userId, product);
                }
                else
                {
                    return await HandleAddToCartWithoutSizeAsync(addToCartDto, userId, product);
                }
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "An error occurred while adding the item to the cart.",
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                };
            }
        }

        private async Task<GeneralResponse<bool>> HandleAddToCartWithSizeAsync(AddToCartDTO addToCartDto, string userId, Product product)
        {
            if (addToCartDto.SizeId == 0)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "You should select the size of the product."
                };
            }

            if (addToCartDto.Quantity == 0)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "You should select the quantity of the product."
                };
            }

            var productSize = await unitOfWork.GetRepository<ProductSize>().FindAsync(p => p.ProductId == addToCartDto.ProductId && p.SizeId == addToCartDto.SizeId);

            if (productSize is null)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "You should select a valid size."
                };
            }

            if (productSize.StockQuantity < addToCartDto.Quantity)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "Not enough stock.",
                    Errors = new List<string> { $"Only {productSize?.StockQuantity ?? 0} units of this product are available." }
                };
            }

            return await AddOrUpdateCartItemAsync(addToCartDto, userId, product, productSize.StockQuantity, productSize.Id);
        }


        private async Task<GeneralResponse<bool>> HandleAddToCartWithoutSizeAsync(AddToCartDTO addToCartDto, string userId, Product product)
        {
            if (addToCartDto.Quantity == 0)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "You should select the quantity of the product."
                };
            }

            if (addToCartDto.Quantity > product.StockQuantity)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "Not enough stock.",
                    Errors = new List<string> { $"Only {product.StockQuantity} units of this product are available." }
                };
            }

            return await AddOrUpdateCartItemAsync(addToCartDto, userId, product, product.StockQuantity, null);
        }


        private async Task<GeneralResponse<bool>> AddOrUpdateCartItemAsync(AddToCartDTO addToCartDto, string userId, Product product, int availableStock, int? productSizeId = null)
        {
            Cart cart = await unitOfWork.cartRepository.FindAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await unitOfWork.cartRepository.AddAsync(cart);
                await unitOfWork.SaveChangesAsync();
            }

            CartItem cartItem = await unitOfWork.cartItemRepository.FindAsync(
                item => item.CartId == cart.CartId && item.ProductId == addToCartDto.ProductId && item.ProductSizeId == productSizeId
            );

            if (cartItem != null)
            {
                cartItem.Quantity += addToCartDto.Quantity;

                if (cartItem.Quantity > availableStock)
                {
                    int availableToAdd = availableStock - (cartItem.Quantity - addToCartDto.Quantity);

                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = $"You can only add {availableToAdd} more units.",
                        Errors = new List<string> { $"You can only add {availableToAdd} more units." }
                    };
                }
            }
            else
            {
                addToCartDto.CartId = cart.CartId;
                cartItem = mapper.Map<CartItem>(addToCartDto);
                cartItem.ProductSizeId = productSizeId;
                await unitOfWork.cartItemRepository.AddAsync(cartItem);
            }

            await unitOfWork.SaveChangesAsync();

            return new GeneralResponse<bool>
            {
                Succeeded = true,
                Message = "Item added successfully."
            };
        }


        public async Task<GeneralResponse<bool>> RemoveFromCartAsync(RemoveFromCartDto removeFromCartDto, string userId)
        {
            try
            {
                // Find the cart item based on the cart ID, product ID, and optional product size
                var cart = await unitOfWork.cartRepository.FindAsync(c => c.UserId == userId);
                var cartItem = await unitOfWork.cartItemRepository.FindAsync(
        item => item.CartId == cart.CartId &&
                item.ProductId == removeFromCartDto.ProductId &&
                (removeFromCartDto.ProductSizeId == 0 || item.ProductSizeId == removeFromCartDto.ProductSizeId));


                if (cartItem == null)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = "Item not found."
                    };
                }

                cartItem.Quantity -= removeFromCartDto.Quantity;

                if (cartItem.Quantity <= 0)
                {
                    await unitOfWork.cartItemRepository.DeleteAsync(cartItem.CartItemId);
                }
                else
                {
                    unitOfWork.cartItemRepository.Update(cartItem);
                }

                await unitOfWork.SaveChangesAsync();

                return new GeneralResponse<bool>
                {
                    Succeeded = true,
                    Message = cartItem.Quantity <= 0 ? "Item removed from cart successfully." : "Item quantity updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "An error occurred while updating the cart.",
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                };
            }
        }


    }
}
