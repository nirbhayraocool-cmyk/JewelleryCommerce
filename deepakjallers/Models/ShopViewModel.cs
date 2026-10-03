namespace JewelleryCommerce.Web.Models
{
    public class ShopViewModel
    {
        public List<ProductCardVM> Products { get; set; } = new();
        public List<CategoryFilterVM> Categories { get; set; } = new();
        public List<VendorFilterVM> Vendors { get; set; } = new();

        // Active filters
        public string? CategorySlug { get; set; }
        public List<int> VendorIds { get; set; } = new();
        public List<string> MetalTypes { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ProductCardVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Mrp { get; set; }
        public string Purity { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsBestseller { get; set; }
    }

    public class CategoryFilterVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class VendorFilterVM
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}