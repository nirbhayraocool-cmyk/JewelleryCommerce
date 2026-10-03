using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models;
using JewelleryCommerce.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var p = await _db.Products
                .Include(x => x.Vendor)
                .Include(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Variants)
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(x => x.Slug == slug && x.IsApproved && x.IsActive);

            if (p == null) return NotFound();

            // Rating breakdown
            var approvedReviews = p.Reviews.Where(r => r.IsApproved).ToList();
            var breakdown = new Dictionary<int, int>();
            for (int i = 5; i >= 1; i--)
                breakdown[i] = approvedReviews.Count(r => r.Rating == i);

            var model = new ProductDetailViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Purity = p.Purity,
                Weight = p.Weight,
                Price = p.Price,
                Mrp = p.Mrp,
                Stock = p.Stock,
                MetalType = p.MetalType.ToString(),
                VendorId = p.VendorId,
                VendorName = p.Vendor.ShopName,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                Images = p.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .ToList(),
                Variants = p.Variants.OrderBy(v => v.Id).ToList(),
                AverageRating = approvedReviews.Any()
                    ? approvedReviews.Average(r => r.Rating) : 0,
                TotalReviews = approvedReviews.Count,
                RatingBreakdown = breakdown,
                Reviews = approvedReviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewItemVM
                    {
                        Id = r.Id,
                        UserName = r.User.FullName,
                        Rating = r.Rating,
                        Title = r.Title,
                        Comment = r.Comment,
                        IsVerifiedPurchase = r.IsVerifiedPurchase,
                        CreatedAt = r.CreatedAt
                    }).ToList()
            };

            // Related products (same category)
            model.RelatedProducts = await _db.Products
                .Include(x => x.Vendor)
                .Include(x => x.Images)
                .Where(x => x.CategoryId == p.CategoryId
                            && x.Id != p.Id
                            && x.IsApproved
                            && x.IsActive)
                .OrderByDescending(x => x.Rating)
                .Take(4)
                .Select(x => new ProductCardVM
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    ImageUrl = x.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                               ?? x.Images.Select(i => i.ImageUrl).FirstOrDefault()
                               ?? "https://via.placeholder.com/400",
                    VendorName = x.Vendor.ShopName,
                    Price = x.Price,
                    Mrp = x.Mrp,
                    Purity = x.Purity,
                    Weight = x.Weight,
                    Rating = x.Rating,
                    ReviewCount = x.ReviewCount,
                    IsBestseller = x.IsBestseller
                })
                .ToListAsync();

            return View(model);
        }
    }
}