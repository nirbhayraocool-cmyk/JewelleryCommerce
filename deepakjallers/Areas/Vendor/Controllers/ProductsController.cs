using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VendorEntity = JewelleryCommerce.Web.Models.Entities.Vendor;

namespace JewelleryCommerce.Web.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(Policy = "VendorOnly")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<VendorEntity?> GetCurrentVendorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);
        }

        public async Task<IActionResult> Index()
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var products = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.VendorId == vendor.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Vendor = vendor;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, string? ImageUrl, int Stock)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            ModelState.Remove("Vendor");
            ModelState.Remove("Category");
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }

            var product = new Product
            {
                VendorId = vendor.Id,
                CategoryId = model.CategoryId,
                Title = model.Title,
                Slug = GenerateSlug(model.Title),
                Description = model.Description,
                MetalType = model.MetalType,
                Purity = model.Purity,
                Weight = model.Weight,
                Price = model.Price,
                Mrp = model.Mrp,
                Stock = Stock,
                IsApproved = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Images = string.IsNullOrWhiteSpace(ImageUrl)
                    ? new List<ProductImage>()
                    : new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = ImageUrl, IsPrimary = true }
                    }
            };

            try
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message + " || " + ex.InnerException?.Message;
                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }

            TempData["Success"] = "Product submitted for approval";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);

            if (product == null) return NotFound();

            await LoadCategoriesAsync(product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, string? ImageUrl)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);

            if (product == null) return NotFound();

            ModelState.Remove("Vendor");
            ModelState.Remove("Category");
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model.CategoryId);
                return View(model);
            }

            product.CategoryId = model.CategoryId;
            product.Title = model.Title;
            product.Description = model.Description;
            product.MetalType = model.MetalType;
            product.Purity = model.Purity;
            product.Weight = model.Weight;
            product.Price = model.Price;
            product.Mrp = model.Mrp;
            product.Stock = model.Stock;
            product.IsApproved = false;

            if (!string.IsNullOrWhiteSpace(ImageUrl))
            {
                var primary = product.Images.FirstOrDefault(i => i.IsPrimary);
                if (primary != null) primary.ImageUrl = ImageUrl;
                else product.Images.Add(new ProductImage { ImageUrl = ImageUrl, IsPrimary = true });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Product updated and sent for re-approval";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);

            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product deleted";
            return RedirectToAction("Index");
        }

        private async Task LoadCategoriesAsync(int selectedId = 0)
        {
            ViewBag.Categories = new SelectList(
                await _db.Categories.OrderBy(c => c.DisplayOrder).ToListAsync(),
                "Id", "Name", selectedId);
        }

        private static string GenerateSlug(string title)
        {
            var slug = title.ToLower()
                .Replace(" ", "-")
                .Replace(",", "")
                .Replace(".", "");
            var chars = slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray();
            return new string(chars) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }
    }
}