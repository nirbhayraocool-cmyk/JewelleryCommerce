using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using JewelleryCommerce.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
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
            var customerIds = await _userManager.GetUsersInRoleAsync("Customer");

            var vm = new AdminDashboardViewModel
            {
                TotalVendors = await _db.Vendors.CountAsync(),
                PendingVendors = await _db.Vendors.CountAsync(v => v.Status == VendorStatus.Pending),
                TotalProducts = await _db.Products.CountAsync(),
                PendingProducts = await _db.Products.CountAsync(p => !p.IsApproved),
                TotalOrders = await _db.Orders.CountAsync(),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                TotalCustomers = customerIds.Count,
                TotalRevenue = await _db.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalCommission = await _db.OrderItems
                    .Include(oi => oi.Vendor)
                    .SumAsync(oi => (decimal?)(oi.Price * oi.Vendor.CommissionRate / 100)) ?? 0,

                RecentOrders = await _db.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOrderVM
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.CustomerName,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status.ToString(),
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync(),

                TopVendors = await _db.Vendors
                    .Where(v => v.Status == VendorStatus.Approved)
                    .Select(v => new TopVendorVM
                    {
                        VendorId = v.Id,
                        ShopName = v.ShopName,
                        ProductCount = v.Products.Count(),
                        OrdersCount = _db.OrderItems.Count(oi => oi.VendorId == v.Id),
                        Revenue = _db.OrderItems
                            .Where(oi => oi.VendorId == v.Id)
                            .Sum(oi => (decimal?)oi.Price) ?? 0
                    })
                    .OrderByDescending(v => v.Revenue)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}