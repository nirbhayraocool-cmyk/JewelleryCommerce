using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var cats = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Product count per category
            var counts = await _db.Products
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ProductCounts = counts.ToDictionary(x => x.CategoryId, x => x.Count);
            return View(cats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string slug, string? imageUrl, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            {
                TempData["Error"] = "Name and slug are required";
                return RedirectToAction("Index");
            }

            var slugClean = slug.Trim().ToLower();

            // Check if slug already exists
            if (await _db.Categories.AnyAsync(c => c.Slug == slugClean))
            {
                TempData["Error"] = $"Category with slug '{slugClean}' already exists!";
                return RedirectToAction("Index");
            }

            _db.Categories.Add(new Category
            {
                Name = name.Trim(),
                Slug = slugClean,
                ImageUrl = imageUrl?.Trim(),
                DisplayOrder = displayOrder
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category added";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Cannot delete — products exist in this category";
                return RedirectToAction("Index");
            }

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category deleted";
            return RedirectToAction("Index");
        }
    }
}