using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Application.Interfaces.Repository
{
    public interface IAdminRepository
    {
        public Task<IQueryable<Seller>> getAllSellersAsync(SellerAccountStatus? status, bool? isBlocked);

    }
}
