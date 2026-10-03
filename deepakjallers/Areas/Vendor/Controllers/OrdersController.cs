using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(Policy = "VendorOnly")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor == null) return NotFound();

            var query = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Images)
                .Where(oi => oi.VendorId == vendor.Id);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderItemStatus>(status, out var st))
                query = query.Where(oi => oi.Status == st);

            var items = await query
                .OrderByDescending(oi => oi.Order.CreatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor == null) return NotFound();

            var item = await _db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id && oi.VendorId == vendor.Id);

            if (item == null) return NotFound();

            if (Enum.TryParse<OrderItemStatus>(status, out var newStatus))
            {
                item.Status = newStatus;

                // Auto-update parent order status if ALL items delivered
                var allItems = await _db.OrderItems
                    .Where(oi => oi.OrderId == item.OrderId)
                    .ToListAsync();

                if (allItems.All(x => x.Status == OrderItemStatus.Delivered))
                    item.Order.Status = OrderStatus.Delivered;
                else if (allItems.All(x => x.Status == OrderItemStatus.Cancelled))
                    item.Order.Status = OrderStatus.Cancelled;
                else if (allItems.Any(x => x.Status == OrderItemStatus.Shipped))
                    item.Order.Status = OrderStatus.Shipped;

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Status updated to {status}";
            }

            return RedirectToAction("Index");
        }
    }
}