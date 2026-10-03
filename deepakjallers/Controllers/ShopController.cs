using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models;
using JewelleryCommerce.Web.Models.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;

        public ShopController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            string? cat,
            List<int>? vendor,
            List<string>? metal,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            int pageSize = 12;
            if (page < 1) page = 1;

            var query = _db.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsApproved && p.IsActive)
                .AsQueryable();

            // Category filter
            if (!string.IsNullOrEmpty(cat))
                query = query.Where(p => p.Category.Slug == cat);

            // Vendor filter
            if (vendor != null && vendor.Any())
                query = query.Where(p => vendor.Contains(p.VendorId));

            // Metal filter
            if (metal != null && metal.Any())
            {
                var metals = metal
                    .Select(m => Enum.TryParse<MetalType>(m, out var mt) ? (MetalType?)mt : null)
                    .Where(m => m.HasValue)
                    .Select(m => m!.Value)
                    .ToList();
                if (metals.Any())
                    query = query.Where(p => metals.Contains(p.MetalType));
            }

            // Price filter
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Sorting
            query = sort switch
            {
                "price_low" => query.OrderBy(p => p.Price),
                "price_high" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "rating" => query.OrderByDescending(p => p.Rating),
                _ => query.OrderByDescending(p => p.IsBestseller)
                          .ThenByDescending(p => p.Rating)
            };

            var total = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardVM
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                               ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault()
                               ?? "https://via.placeholder.com/400",
                    VendorName = p.Vendor.ShopName,
                    Price = p.Price,
                    Mrp = p.Mrp,
                    Purity = p.Purity,
                    Weight = p.Weight,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    IsBestseller = p.IsBestseller
                })
                .ToListAsync();

            var categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryFilterVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ProductCount = c.Products.Count(p => p.IsApproved && p.IsActive)
                })
                .ToListAsync();

            var vendors = await _db.Vendors
                .Where(v => v.Status == VendorStatus.Approved)
                .Select(v => new VendorFilterVM
                {
                    Id = v.Id,
                    ShopName = v.ShopName,
                    ProductCount = v.Products.Count(p => p.IsApproved && p.IsActive)
                })
                .ToListAsync();

            var model = new ShopViewModel
            {
                Products = products,
                Categories = categories,
                Vendors = vendors,
                CategorySlug = cat,
                VendorIds = vendor ?? new(),
                MetalTypes = metal ?? new(),
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sort ?? "popular",
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return View(model);
        }
    }
}