using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using JewelleryCommerce.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(Policy = "VendorOnly")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor == null) return NotFound("Vendor profile not found");

            var vendorItems = _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Images)
                .Where(oi => oi.VendorId == vendor.Id);

            var totalEarnings = await vendorItems.SumAsync(oi => (decimal?)oi.VendorEarning) ?? 0;

            var pendingEarnings = await vendorItems
                .Where(oi => oi.Status == OrderItemStatus.Pending
                          || oi.Status == OrderItemStatus.Confirmed
                          || oi.Status == OrderItemStatus.Packed
                          || oi.Status == OrderItemStatus.Shipped)
                .SumAsync(oi => (decimal?)oi.VendorEarning) ?? 0;

            var paidEarnings = await vendorItems
                .Where(oi => oi.Status == OrderItemStatus.Delivered)
                .SumAsync(oi => (decimal?)oi.VendorEarning) ?? 0;

            var vm = new VendorDashboardViewModel
            {
                ShopName = vendor.ShopName,
                CommissionRate = vendor.CommissionRate,
                TotalProducts = await _db.Products.CountAsync(p => p.VendorId == vendor.Id),
                ApprovedProducts = await _db.Products.CountAsync(p => p.VendorId == vendor.Id && p.IsApproved),
                PendingProducts = await _db.Products.CountAsync(p => p.VendorId == vendor.Id && !p.IsApproved),
                TotalOrders = await vendorItems.CountAsync(),
                PendingOrders = await vendorItems.CountAsync(oi =>
                    oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Confirmed),
                DeliveredOrders = await vendorItems.CountAsync(oi => oi.Status == OrderItemStatus.Delivered),
                TotalEarnings = totalEarnings,
                PendingEarnings = pendingEarnings,
                PaidEarnings = paidEarnings,

                RecentOrders = await vendorItems
                    .OrderByDescending(oi => oi.Order.CreatedAt)
                    .Take(8)
                    .Select(oi => new RecentVendorOrderVM
                    {
                        OrderItemId = oi.Id,
                        OrderId = oi.OrderId,
                        OrderNumber = oi.Order.OrderNumber,
                        CustomerName = oi.Order.CustomerName,
                        ProductTitle = oi.Product.Title,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        VendorEarning = oi.VendorEarning,
                        Status = oi.Status.ToString(),
                        CreatedAt = oi.Order.CreatedAt
                    })
                    .ToListAsync(),

                TopProducts = await vendorItems
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopProductVM
                    {
                        ProductId = g.Key,
                        Title = g.First().Product.Title,
                        ImageUrl = g.First().Product.Images
                            .Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                            ?? g.First().Product.Images.Select(i => i.ImageUrl).FirstOrDefault()
                            ?? "https://via.placeholder.com/50",
                        SoldCount = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
