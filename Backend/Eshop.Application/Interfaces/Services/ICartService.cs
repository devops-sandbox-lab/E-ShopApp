using Eshop.Application.DTOs.Cart;
using Eshop.Application.GeneralResponse;

namespace Eshop.Application.Interfaces.Services
{
    public interface ICartService
    {
        public Task<GeneralResponse<CartDTO>> GetCartByUserIdAsync(string userId, int page, int pageSize);
        Task<GeneralResponse<bool>> AddToCartAsync(AddToCartDTO addToCartDto, string UserId);
        /*        Task<GeneralResponse> UpdateCartItemAsync(UpdateCartItemDto updateCartItemDto);*/
        public Task<GeneralResponse<bool>> RemoveFromCartAsync(RemoveFromCartDto removeFromCartDto, string userId);
    }
}
