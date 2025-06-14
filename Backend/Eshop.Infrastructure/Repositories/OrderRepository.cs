using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
