using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Vendor? Vendor { get; set; }
    }
}