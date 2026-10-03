namespace JewelleryCommerce.Web.Models.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;
    }
}