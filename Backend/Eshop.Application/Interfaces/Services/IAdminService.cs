using Eshop.Application.DTOs.Profile;
using Eshop.Application.GeneralResponse;
using Eshop.Core.Enums;

namespace Eshop.Application.Interfaces.Services
{
    public interface IAdminService
    {
        public Task<GeneralResponse<object>> AcceptSellerAccount(string sellerId);
        public Task<GeneralResponse<List<GetSellerProfileDTO>>> GetAllSellers(int page, int pageSize, SellerAccountStatus? status, bool? isBlocked);
        public Task<GeneralResponse<object>> DeclineSellerAccount(string sellerId);
    }
}
