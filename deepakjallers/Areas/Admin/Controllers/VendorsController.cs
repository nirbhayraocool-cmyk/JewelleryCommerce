using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class VendorsController : Controller
    {
        private readonly AppDbContext _db;

        public VendorsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.Vendors
                .Include(v => v.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<VendorStatus>(status, out var st))
                query = query.Where(v => v.Status == st);

            var vendors = await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new
                {
                    v.Id,
                    v.ShopName,
                    v.GstNumber,
                    v.Status,
                    v.CommissionRate,
                    v.CreatedAt,
                    UserEmail = v.User.Email,
                    FullName = v.User.FullName,
                    ProductCount = v.Products.Count()
                })
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(vendors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var v = await _db.Vendors.FindAsync(id);
            if (v == null) return NotFound();
            v.Status = VendorStatus.Approved;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{v.ShopName} approved successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var v = await _db.Vendors.FindAsync(id);
            if (v == null) return NotFound();
            v.Status = VendorStatus.Rejected;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{v.ShopName} rejected";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id)
        {
            var v = await _db.Vendors.FindAsync(id);
            if (v == null) return NotFound();
            v.Status = VendorStatus.Suspended;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{v.ShopName} suspended";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCommission(int id, decimal rate)
        {
            var v = await _db.Vendors.FindAsync(id);
            if (v == null) return NotFound();
            if (rate < 0 || rate > 50) rate = 10;
            v.CommissionRate = rate;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Commission updated to {rate}%";
            return RedirectToAction("Index");
        }
    }
}