using Microsoft.EntityFrameworkCore;
using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public ApplicationDbContext _context { get; }

        public async Task DeleteImagesAsync(List<int> ids)
        {
            var images = await _context.ProductImages.Where(img => ids.Contains(img.Id)).ToListAsync();

            if (images.Any())
            {
                _context.ProductImages.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProductImages> GetImageByID(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public async Task<bool> IsFav(int productId, string UserId)
        {
            return await _context.FavoriteService.AnyAsync(p => p.ProductId == productId && p.CustomerId == UserId);
        }

        /*        public async Task<bool> isPreviouslyBought(int productId, string UserId)
                {
                    return await _context.FavoriteService.AnyAsync(p => p.ProductId == productId && p.CustomerId == UserId);
                }*/
    }
}
