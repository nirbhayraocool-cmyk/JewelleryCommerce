using JewelleryCommerce.Web.Models.Entities;

namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Purity { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
        public decimal Mrp { get; set; }
        public int DiscountPercent => Mrp > 0 && Mrp > Price
            ? (int)Math.Round((1 - (double)(Price / Mrp)) * 100) : 0;
        public int Stock { get; set; }
        public string MetalType { get; set; } = string.Empty;

        // Vendor
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;

        // Category
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;

        // Images
        public List<ProductImage> Images { get; set; } = new();

        // Variants
        public List<ProductVariant> Variants { get; set; } = new();

        // Reviews
        public List<ReviewItemVM> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new();

        // Related
        public List<ProductCardVM> RelatedProducts { get; set; } = new();
    }

    public class ReviewItemVM
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}