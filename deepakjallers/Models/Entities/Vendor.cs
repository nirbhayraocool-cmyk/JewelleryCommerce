using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string ShopName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? GstNumber { get; set; }

        public string? LogoUrl { get; set; }

        public VendorStatus Status { get; set; } = VendorStatus.Pending;

        public decimal CommissionRate { get; set; } = 10m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}