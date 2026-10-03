using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Value { get; set; }

        public decimal? AdditionalPrice { get; set; }

        public int Stock { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}