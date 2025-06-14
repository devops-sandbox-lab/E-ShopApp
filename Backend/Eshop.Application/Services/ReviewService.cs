using Application.Helpers;
using AutoMapper;
using Eshop.Application.DTOs;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;

namespace Eshop.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<bool> AddReview(AddReviewDTO addReview, string userId)
        {
            var customer = await unitOfWork.GetRepository<Customer>().FindAsync(c => c.Id == userId);
            if (customer == null)
            {
                return false;
            }



            var review = mapper.Map<Reviews>(addReview);
            if (addReview.ReviewImage != null)
            {
                var imgUrl = await ImageSavingHelper.SaveOneImageAsync(addReview.ReviewImage, "ReviewsImages");
                review.reviewImageUrl = imgUrl;
            }
            try
            {
                var orders = await unitOfWork.orderRepository.FindAllAsync(
                    new[] { "OrderItems" },
                    order => order.BuyerId == customer.Id
                );

                bool hasPurchasedProduct = orders
                    .SelectMany(order => order.OrderItems)
                    .Any(item => item.ProductId == addReview.ProductId);


                var product = await unitOfWork.productRepository.FindAsync(p => p.ProductId == addReview.ProductId);
                if (hasPurchasedProduct && product is not null)
                {
                    review.customerName = $"{customer.FirstName} {customer.LastName}";
                    review.customerImage = customer.ProfileImage;
                    review.CustomerId = customer.Id;

                    unitOfWork.GetRepository<Reviews>().Add(review);

                    var reviews = await unitOfWork.GetRepository<Reviews>()
                        .FindAllAsync(null, r => r.ProductId == addReview.ProductId);

                    product.Rating = Math.Ceiling(reviews.Average(r => r.Rating));



                    await unitOfWork.productRepository.UpdateAsync(product);
                    await unitOfWork.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

    }
}
