using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMode PaymentMode { get; set; } = PaymentMode.COD;

        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}