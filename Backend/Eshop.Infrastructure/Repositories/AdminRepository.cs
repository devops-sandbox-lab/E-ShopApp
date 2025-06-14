using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext context;

        public AdminRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IQueryable<Seller>> getAllSellersAsync(SellerAccountStatus? status, bool? isBlocked)
        {
            var query = context.Sellers.AsQueryable();


            if (status.HasValue)
            {
                query = query.Where(o => o.AccountStatus == status.Value);
            }

            if (isBlocked.HasValue)
            {
                query = query.Where(o => o.IsBlocked == isBlocked.Value);
            }

            query = query.OrderByDescending(o => o.DateCreated);
            return query;
        }
    }
}
