namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);
        public decimal Total => Subtotal;
        public int TotalItems => Items.Sum(i => i.Quantity);
    }

    public class CartItemVM
    {
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Mrp { get; set; }
        public int Quantity { get; set; }
        public int MaxStock { get; set; }
        public string? VariantName { get; set; }
        public decimal LineTotal => Price * Quantity;
    }

    // Session me store karne ke liye (lightweight)
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? VariantId { get; set; }
    }
}