using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Slug { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}