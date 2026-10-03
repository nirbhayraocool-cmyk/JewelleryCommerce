using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public MetalType MetalType { get; set; }

        [MaxLength(20)]
        public string Purity { get; set; } = string.Empty;

        public decimal Weight { get; set; }

        public decimal Price { get; set; }
        public decimal Mrp { get; set; }

        public int Stock { get; set; }

        public double Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        public bool IsBestseller { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}