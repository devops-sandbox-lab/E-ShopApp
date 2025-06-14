using AutoMapper;
using Eshop.Application.DTOs;
using Eshop.Application.DTOs.Cart;
using Eshop.Application.DTOs.Order;
using Eshop.Application.DTOs.Product;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.Helpers;
using Eshop.Core.Entities;

namespace Eshop.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Seller, SellerRegisterDTO>().ReverseMap()
                .ForMember(des => des.TaxRegisterPDF, opt => opt.MapFrom(src => src.TaxRegisterPDF));
            CreateMap<Customer, CustomerRegisterDTO>()
                .ForMember(des => des.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ReverseMap();

            CreateMap<Category, CategoryDto>();
            CreateMap<SubCategory, SubCategoryDto>();
            CreateMap<Category, CategoryWithSubCategoriesDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

            CreateMap<Size, GetSizeDTO>().ReverseMap();

            CreateMap<Product, AddProductDTO>()
            .ForMember(dest => dest.productImagesUrl, opt => opt.MapFrom(src => src.productImages.Select(p => p.ImageUrl).ToList()))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
            .ForMember(dest => dest.productImages, opt => opt.Ignore());


            CreateMap<Product, EditProductDTO>()
            .ForMember(dest => dest.productImagesUrl, opt => opt.MapFrom(src => src.productImages.Select(p => p.ImageUrl).ToList()))
            .ForMember(dest => dest.productImages, opt => opt.Ignore());


            CreateMap<AddProductDTO, Product>()
                .ForMember(dest => dest.productImages, opt => opt.MapFrom(src => src.productImagesUrl.Select(url => new ProductImages { ImageUrl = url }).ToList()))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                .ForMember(dest => dest.ProductSizes, opt => opt.Ignore())
                    .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src =>
        src.ProductSizes != null && src.ProductSizes.Any() ? 0 : src.StockQuantity ?? 0));


            CreateMap<EditProductDTO, Product>()
               .ForMember(dest => dest.productImages, opt => opt.MapFrom(src => src.productImagesUrl.Select(url => new ProductImages { ImageUrl = url }).ToList()))
               .ForMember(dest => dest.ProductSizes, opt => opt.Ignore())
                   .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src =>
       src.ProductSizes != null && src.ProductSizes.Any() ? 0 : src.StockQuantity ?? 0));

            CreateMap<ProductSizeDTO, ProductSize>();

            CreateMap<ProductSize, ProductSizeDetailDTO>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Size.Name));


            CreateMap<Product, ProductDTO>()
          .ForMember(dest => dest.productImagesUrl, opt => opt.MapFrom(src => src.productImages.Select(p => p.ImageUrl).ToList()))
          .ForMember(dest => dest.ProductSizes, opt => opt.Ignore());

            CreateMap<Feature, AddProductFeaturesDTO>().ReverseMap();

            CreateMap<Feature, EditFeatureDTO>().ReverseMap();


            CreateMap<Product, GetProductsDTO>()
         .ForMember(dest => dest.productImagesUrl, opt => opt.MapFrom(src => src.productImages.Select(img => img.ImageUrl).ToList()))
         .ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.Features.Select(f => new GetProductFeaturesDTO
         {
             FeatureId = f.FeatureId,
             FeatureName = f.FeatureName,
             FeatureValue = f.FeatureValue
         }).ToList()))
         .ForMember(dest => dest.reviews, opt => opt.Ignore())
         .ForMember(dest => dest.ProductSizes, opt => opt.MapFrom(src => src.ProductSizes.Select(p => new ProductSizeDTO
         {
             Price = p.Price,
             SizeId = p.SizeId,
             StockQuantity = p.StockQuantity,
         })));

            CreateMap<Seller, GetSellerProfileDTO>()
             .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.Id));

            // Mapping from UpdateSellerProfileDTO to Seller
            CreateMap<UpdateSellerProfileDTO, Seller>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                           .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.imgURL))
                .ForMember(dest => dest.HasOwnShipping, opt => opt.MapFrom(src => src.HasOwnShipping));

            CreateMap<Customer, GetCustomerProfileDTO>()
             .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id)).ReverseMap();

            CreateMap<UpdateCustomerProfileDTO, Customer>()
              .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
              .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
              .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                         .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.imgURL));


            //Cart 

            CreateMap<CartItem, AddToCartDTO>().ReverseMap();

            CreateMap<Cart, CartDTO>();

            CreateMap<CartItem, CartItemDTO>()
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
          .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductSize != null ? src.ProductSize.Price : src.Product.Price))
          .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
          .ForMember(dest => dest.SizeId, opt => opt.MapFrom(src => src.ProductSize != null ? src.ProductSize.SizeId : (int?)null))
          .ForMember(dest => dest.SampleImage, opt => opt.MapFrom(src => src.Product.productImages[0].ImageUrl))
  .ForMember(dest => dest.AllQuantityAvalibale, opt => opt.MapFrom(src =>
    ProductSizeHelper.RequiresSizes(src.Product.CategoryId)
        ? src.Product.ProductSizes
              .FirstOrDefault(p => p.SizeId == src.ProductSize.SizeId).StockQuantity
        : src.Product.StockQuantity
));



            //Order
            CreateMap<Order, OrderDTO>()
           .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.BuyerId))
           .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId));

            CreateMap<OrderDTO, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.Product.sellerId))
                .ForMember(dest => dest.ProductSizeId, opt => opt.MapFrom(src => src.ProductSizeId));
            CreateMap<OrderItemDTO, OrderItem>();


            //OrderTemp
            CreateMap<TempOrder, OrderDTO>()
           .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.TempOrderItems))
           .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.TempOrderId))
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.BuyerId))
           .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.SellerId));

            CreateMap<OrderDTO, TempOrder>()
                .ForMember(dest => dest.TempOrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<TempOrderItem, OrderItemDTO>()
                .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.Product.sellerId))
                .ForMember(dest => dest.ProductSizeId, opt => opt.MapFrom(src => src.ProductSizeId));
            CreateMap<OrderItemDTO, TempOrderItem>();


            //Reviews
            CreateMap<Reviews, AddReviewDTO>();
            CreateMap<AddReviewDTO, Reviews>();


            CreateMap<TempOrder, Order>()
        .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            CreateMap<TempOrderItem, OrderItem>();
        }
    }
}
