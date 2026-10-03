using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var st))
                query = query.Where(o => o.Status == st);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Vendor)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (Enum.TryParse<OrderStatus>(status, out var newStatus))
            {
                order.Status = newStatus;
                // Sync item statuses for Delivered/Cancelled
                if (newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled)
                {
                    foreach (var item in order.Items)
                    {
                        item.Status = newStatus == OrderStatus.Delivered
                            ? OrderItemStatus.Delivered
                            : OrderItemStatus.Cancelled;
                    }
                }
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Order {order.OrderNumber} → {status}";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}