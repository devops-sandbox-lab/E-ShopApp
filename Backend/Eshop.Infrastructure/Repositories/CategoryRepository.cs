using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
