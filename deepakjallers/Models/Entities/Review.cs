using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int Rating { get; set; }              // 1-5

        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500)]
        public string Comment { get; set; } = string.Empty;

        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}