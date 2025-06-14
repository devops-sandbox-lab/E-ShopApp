using AutoMapper;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;

namespace Eshop.Application.Services
{
    public class FavoriteService : IFavoriteService
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public FavoriteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<GeneralResponse<bool>> AddProductToFav(int productID, string customerID)
        {
            try
            {
                var product = await unitOfWork.productRepository.FindAsync(p => p.ProductId == productID);

                if (product == null)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = false,
                        Message = "Product not found."
                    };
                }
                if (unitOfWork.favoriteRepository == null)
                {
                    throw new InvalidOperationException("FavoriteRepository is not initialized.");
                }


                var existingFavorite = await unitOfWork.favoriteRepository.FindAsync(f => f.ProductId == productID && f.CustomerId == customerID);

                if (existingFavorite != null)
                {
                    return new GeneralResponse<bool>
                    {
                        Succeeded = true,
                        Message = "Product added to favorites."
                    };
                }

                // Add the product to favorites
                var favorite = new Favorite
                {
                    ProductId = productID,
                    CustomerId = customerID
                };

                unitOfWork.favoriteRepository.Add(favorite);
                await unitOfWork.SaveChangesAsync();

                return new GeneralResponse<bool>
                {
                    Succeeded = true,
                    Message = "Product added to favorites."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    Succeeded = false,
                    Message = "An error occurred while adding the product to favorites.",
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                };
            }
        }



        public async Task<List<int>> GetAllFavIds(string customerID)
        {
            try
            {
                var favoriteProducts = await unitOfWork.GetRepository<Favorite>().FindAllAsync(null, f => f.CustomerId == customerID);

                return favoriteProducts.Select(f => f.ProductId).Distinct().ToList();
            }
            catch (Exception ex)
            {
                return new List<int>();
            }
        }



        public void RemoveProductFromFav(int productID, string customerId)
        {
            Favorite FavService = unitOfWork.favoriteRepository.Find(p => p.ProductId == productID && p.CustomerId == customerId);

            if (FavService != null)
            {
                unitOfWork.favoriteRepository.DeleteAsync(FavService.ID);
                unitOfWork.SaveChangesAsync();
            }
        }


    }
}
