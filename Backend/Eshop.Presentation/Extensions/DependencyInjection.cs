using Eshop.Application.Configurations;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Application.Mapping;
using Eshop.Application.Services;
using Eshop.Infrastructure.Repositories;
using Eshop.Infrastructure;

namespace Eshop.Presentation.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Stripe Payment
            services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
            services.AddScoped<StripeService>();

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Repositories and Services
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.Configure<JWT>(configuration.GetSection("JWT"));
            services.Configure<MailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserOTPRepository, UserOTPRepository>();
            services.AddScoped<IUserOTPService, UserOTPService>();
            services.Configure<GoogleAuthConfig>(configuration.GetSection("Google"));
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IFeatureRepository, FeatureRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
