using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<UserOTP> userOTPs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Cart> carts { get; set; }
        public DbSet<Favorite> FavoriteService { get; set; }
        public DbSet<Reviews> Reviews { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<TempOrder> tempOrders { get; set; }
        public DbSet<TempOrderItem> tempOrderItems { get; set; }
        public DbSet<PurchaseItem> purchaseItems { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
          .ToTable("Customers");

            modelBuilder.Entity<Seller>()
                .ToTable("Sellers");

            modelBuilder.Entity<Category>()
           .HasMany(c => c.SubCategories)
           .WithOne(sc => sc.Category)
           .HasForeignKey(sc => sc.CategoryId);

            modelBuilder.Entity<SubCategory>()
                .HasMany(sc => sc.Products)
                .WithOne(p => p.SubCategory)
                .HasForeignKey(p => p.SubCategoryId);


            // TO CONfing the many-to-many relationship between Product and Size 
            modelBuilder.Entity<ProductSize>()
          .HasKey(ps => ps.Id);

            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId);

            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Size)
                .WithMany(s => s.ProductSizes)
                .HasForeignKey(ps => ps.SizeId);



            modelBuilder.Entity<Feature>()
                .HasOne(f => f.Product)
                .WithMany(f => f.Features)
                .HasForeignKey(f => f.ProductId);

            modelBuilder.Entity<ProductImages>()
              .HasOne(pi => pi.product)
              .WithMany(p => p.productImages)
              .HasForeignKey(pi => pi.productId);

            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent NoAction delete

            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent NoAction delete

            // Composite unique constraint to ensure one review per customer per product
            modelBuilder.Entity<Reviews>()
                .HasIndex(r => new { r.CustomerId, r.ProductId })
                .IsUnique();

            modelBuilder.Entity<Cart>()
        .HasMany(c => c.CartItems)
        .WithOne(ci => ci.Cart)
        .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
            modelBuilder.Entity<OrderItem>()
        .Ignore(oi => oi.ProductSize);



            modelBuilder.Entity<Category>().HasData(
       new Category { CategoryId = 1, Name = "Men" },
       new Category { CategoryId = 2, Name = "Women" },
       new Category { CategoryId = 3, Name = "Girls" },
       new Category { CategoryId = 4, Name = "Boys" },
       new Category { CategoryId = 5, Name = "Kids" },
       new Category { CategoryId = 6, Name = "Electronics" },
       new Category { CategoryId = 7, Name = "Makeup" },
           new Category { CategoryId = 8, Name = "Others" }
   );
            modelBuilder.Entity<Size>().HasData(
                new Size { SizeId = 1, Name = "S" },
                new Size { SizeId = 2, Name = "M" },
                new Size { SizeId = 3, Name = "L" },
                new Size { SizeId = 4, Name = "XS" },
                new Size { SizeId = 5, Name = "XL" },
                new Size { SizeId = 6, Name = "XXL" },
                new Size { SizeId = 7, Name = "XXXL" },
                new Size { SizeId = 8, Name = "XXXXL" }
            );

            for (int i = 1; i <= 50; i++)
            {
                modelBuilder.Entity<Size>().HasData(
                    new Size { SizeId = i + 8, Name = i.ToString() }
                );
            }

            modelBuilder.Entity<SubCategory>().HasData(
                new SubCategory { SubCategoryId = 1, Name = "T-Shirts", CategoryId = 1 },
                new SubCategory { SubCategoryId = 2, Name = "Shirts", CategoryId = 1 },
                new SubCategory { SubCategoryId = 3, Name = "Pants", CategoryId = 1 },
                new SubCategory { SubCategoryId = 4, Name = "Shorts", CategoryId = 1 },
                new SubCategory { SubCategoryId = 5, Name = "Jackets", CategoryId = 1 },
                new SubCategory { SubCategoryId = 6, Name = "Sneakers", CategoryId = 1 },
                new SubCategory { SubCategoryId = 7, Name = "Formal Shoes", CategoryId = 1 },
                new SubCategory { SubCategoryId = 8, Name = "Sandals", CategoryId = 1 },
                new SubCategory { SubCategoryId = 9, Name = "Watches", CategoryId = 1 },
                new SubCategory { SubCategoryId = 10, Name = "Sunglasses", CategoryId = 1 },
                new SubCategory { SubCategoryId = 11, Name = "Belts", CategoryId = 1 },

                new SubCategory { SubCategoryId = 12, Name = "Dresses", CategoryId = 2 },
                new SubCategory { SubCategoryId = 13, Name = "Blouse", CategoryId = 2 },
                new SubCategory { SubCategoryId = 14, Name = "Skirts", CategoryId = 2 },
                new SubCategory { SubCategoryId = 15, Name = "Pants", CategoryId = 2 },
                new SubCategory { SubCategoryId = 16, Name = "Outerwear", CategoryId = 2 },
                new SubCategory { SubCategoryId = 17, Name = "Heels", CategoryId = 2 },
                new SubCategory { SubCategoryId = 18, Name = "Flats", CategoryId = 2 },
                new SubCategory { SubCategoryId = 19, Name = "Sandals", CategoryId = 2 },
                new SubCategory { SubCategoryId = 20, Name = "Jewelry", CategoryId = 2 },
                new SubCategory { SubCategoryId = 21, Name = "Handbags", CategoryId = 2 },
                new SubCategory { SubCategoryId = 22, Name = "Scarves", CategoryId = 2 },

                new SubCategory { SubCategoryId = 23, Name = "Dresses", CategoryId = 3 },
                new SubCategory { SubCategoryId = 24, Name = "Blouse", CategoryId = 3 },
                new SubCategory { SubCategoryId = 25, Name = "Pants", CategoryId = 3 },
                new SubCategory { SubCategoryId = 26, Name = "Skirts", CategoryId = 3 },
                new SubCategory { SubCategoryId = 27, Name = "Shoes", CategoryId = 3 },
                new SubCategory { SubCategoryId = 28, Name = "Sandals", CategoryId = 3 },
                new SubCategory { SubCategoryId = 29, Name = "Hair Accessories", CategoryId = 3 },
                new SubCategory { SubCategoryId = 30, Name = "Bags", CategoryId = 3 },

                new SubCategory { SubCategoryId = 31, Name = "T-Shirts", CategoryId = 4 },
                new SubCategory { SubCategoryId = 32, Name = "Pants", CategoryId = 4 },
                new SubCategory { SubCategoryId = 33, Name = "Shorts", CategoryId = 4 },
                new SubCategory { SubCategoryId = 34, Name = "Sneakers", CategoryId = 4 },
                 new SubCategory { SubCategoryId = 35, Name = "Shoes", CategoryId = 4 },
                new SubCategory { SubCategoryId = 36, Name = "Sandals", CategoryId = 4 },
                new SubCategory { SubCategoryId = 37, Name = "Caps", CategoryId = 4 },
                new SubCategory { SubCategoryId = 38, Name = "Backpacks", CategoryId = 4 },


                new SubCategory { SubCategoryId = 39, Name = "T-Shirts", CategoryId = 5 },
                new SubCategory { SubCategoryId = 40, Name = "Shirts", CategoryId = 5 },
                new SubCategory { SubCategoryId = 41, Name = "Pants", CategoryId = 5 },
                new SubCategory { SubCategoryId = 42, Name = "Shorts", CategoryId = 5 },
                new SubCategory { SubCategoryId = 43, Name = "Socks", CategoryId = 5 },
                new SubCategory { SubCategoryId = 44, Name = "Sleepwear", CategoryId = 5 },
                new SubCategory { SubCategoryId = 45, Name = "Baby Shoes", CategoryId = 5 },
                new SubCategory { SubCategoryId = 46, Name = "Bibs", CategoryId = 5 },
                new SubCategory { SubCategoryId = 47, Name = "Hats", CategoryId = 5 },

                new SubCategory { SubCategoryId = 48, Name = "Mobiles", CategoryId = 6 },
                new SubCategory { SubCategoryId = 49, Name = "Laptops", CategoryId = 6 },
                new SubCategory { SubCategoryId = 50, Name = "Tablets", CategoryId = 6 },
                new SubCategory { SubCategoryId = 51, Name = "Chargers", CategoryId = 6 },
                new SubCategory { SubCategoryId = 52, Name = "Headphones", CategoryId = 6 },
                new SubCategory { SubCategoryId = 53, Name = "Cases", CategoryId = 6 },

                new SubCategory { SubCategoryId = 54, Name = "Foundation", CategoryId = 7 },
                new SubCategory { SubCategoryId = 55, Name = "Concealer", CategoryId = 7 },
                new SubCategory { SubCategoryId = 56, Name = "Blush", CategoryId = 7 },
                new SubCategory { SubCategoryId = 57, Name = "Eyeshadow", CategoryId = 7 },
                new SubCategory { SubCategoryId = 58, Name = "Mascara", CategoryId = 7 },
                new SubCategory { SubCategoryId = 59, Name = "Eyeliner", CategoryId = 7 },
                new SubCategory { SubCategoryId = 60, Name = "Lipstick", CategoryId = 7 },
                new SubCategory { SubCategoryId = 61, Name = "Lip Gloss", CategoryId = 7 },
                new SubCategory { SubCategoryId = 62, Name = "Animals", CategoryId = 8 },
                new SubCategory { SubCategoryId = 63, Name = "Flowers and Guardians", CategoryId = 8 },
                new SubCategory { SubCategoryId = 64, Name = "Home", CategoryId = 8 },
                new SubCategory { SubCategoryId = 65, Name = "Others", CategoryId = 8 }
            );
        }
    }
}
