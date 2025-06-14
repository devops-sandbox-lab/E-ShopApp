namespace Eshop.Core.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string sellerId { get; set; }
        public Seller Seller { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
        public List<Feature> Features { get; set; }
        public List<ProductImages> productImages { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int StockQuantity { get; set; }
        public decimal? Discount { get; set; }
        public int? PurchaseNumber { get; set; }
        public double? Rating { get; set; }
        public string? Brand { get; set; }
        public int? NumberOfRatings { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Reviews>? Reviews { get; set; }

        public bool IsAddedReviewBefore { get; set; }
        public List<ProductSize> ProductSizes { get; set; }
        public List<Favorite> FavoriteProducts { get; set; }

        public static implicit operator List<object>(Product v)
        {
            throw new NotImplementedException();
        }
        public bool IsFavorites { get; set; }
        public bool isPreviouslyBought { get; set; }

        public double DeliveryFee { get; set; }

        public bool IsFreeShipping { get; set; }

    }
}
