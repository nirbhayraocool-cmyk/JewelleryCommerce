using JewelleryCommerce.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        public ProductsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.Products
                .Include(p => p.Vendor)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            if (status == "Pending")
                query = query.Where(p => !p.IsApproved);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.IsApproved = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{p.Title} approved";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.IsApproved = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{p.Title} un-approved";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.IsActive = !p.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{p.Title} is now {(p.IsActive ? "Active" : "Inactive")}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product deleted";
            return RedirectToAction("Index");
        }
    }
}