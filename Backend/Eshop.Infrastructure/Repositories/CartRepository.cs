using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
