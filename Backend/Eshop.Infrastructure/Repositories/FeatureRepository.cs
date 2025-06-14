using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class FeatureRepository : GenericRepository<Feature>, IFeatureRepository
    {
        public FeatureRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
