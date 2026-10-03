using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.Entities;
using JewelleryCommerce.Web.Models.ViewModels;
using JewelleryCommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CartService _cart;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            AppDbContext db,
            CartService cart,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _cart = cart;
            _userManager = userManager;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var cartItems = _cart.GetCart();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var cartVm = await BuildCartViewModel(cartItems);

            var user = await _userManager.GetUserAsync(User);
            var vm = new CheckoutViewModel
            {
                FullName = user?.FullName ?? "",
                Phone = user?.PhoneNumber ?? "",
                Cart = cartVm
            };

            return View(vm);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cartItems = _cart.GetCart();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var cartVm = await BuildCartViewModel(cartItems);
            model.Cart = cartVm;

            if (!ModelState.IsValid)
                return View("Index", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Combine address
            var fullAddress = $"{model.ShippingAddress}, {model.City}, {model.State}";

            // Create order
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = user.Id,
                CustomerName = model.FullName,
                Phone = model.Phone,
                ShippingAddress = fullAddress,
                Pincode = model.Pincode,
                TotalAmount = cartVm.Total,
                Status = OrderStatus.Pending,
                PaymentMode = PaymentMode.COD,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            // Vendor-wise split of items
            foreach (var item in cartVm.Items)
            {
                var product = await _db.Products
                    .Include(p => p.Vendor)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null) continue;

                // Check stock
                if (product.Stock < item.Quantity)
                {
                    ModelState.AddModelError("", $"{product.Title} — only {product.Stock} in stock");
                    return View("Index", model);
                }

                var commission = product.Vendor.CommissionRate;
                var vendorEarning = item.Price * (1 - commission / 100);

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    VendorId = product.VendorId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    VendorEarning = vendorEarning,
                    Status = OrderItemStatus.Pending
                });

                // Reduce stock
                product.Stock -= item.Quantity;
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart
            _cart.Clear();

            return RedirectToAction("Success", new { id = order.Id });
        }

        // GET: /Checkout/Success/5
        public async Task<IActionResult> Success(int id)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.OrderId = order.Id;
            ViewBag.TotalAmount = order.TotalAmount;

            return View();
        }

        // ---------- HELPERS ----------

        private async Task<CartViewModel> BuildCartViewModel(List<CartItemDto> items)
        {
            var productIds = items.Select(i => i.ProductId).ToList();

            var products = await _db.Products
                .Include(p => p.Vendor)
                .Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var vm = new CartViewModel();
            foreach (var item in items)
            {
                var p = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (p == null) continue;

                var image = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                            ?? p.Images.FirstOrDefault()?.ImageUrl
                            ?? "https://via.placeholder.com/200";

                vm.Items.Add(new CartItemVM
                {
                    ProductId = p.Id,
                    VendorId = p.VendorId,
                    VendorName = p.Vendor.ShopName,
                    Title = p.Title,
                    Slug = p.Slug,
                    ImageUrl = image,
                    Price = p.Price,
                    Mrp = p.Mrp,
                    Quantity = item.Quantity,
                    MaxStock = p.Stock
                });
            }
            return vm;
        }

        private string GenerateOrderNumber()
        {
            var random = new Random().Next(1000, 9999);
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{random}";
        }
    }
}
