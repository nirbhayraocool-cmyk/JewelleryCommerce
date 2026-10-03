using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name required")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone required")]
        [Phone]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address required")]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City required")]
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State required")]
        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode required")]
        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public CartViewModel Cart { get; set; } = new();
    }
}