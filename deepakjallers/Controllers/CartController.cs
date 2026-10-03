using JewelleryCommerce.Web.Data;
using JewelleryCommerce.Web.Models.ViewModels;
using JewelleryCommerce.Web.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CartService _cart;

        public CartController(AppDbContext db, CartService cart)
        {
            _db = db;
            _cart = cart;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var items = _cart.GetCart();
            if (!items.Any())
                return View(new CartViewModel());

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

            return View(vm);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, int? variantId = null)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return Json(new { success = false, message = "Product not found" });

            if (quantity < 1) quantity = 1;
            if (quantity > product.Stock) quantity = product.Stock;

            _cart.AddItem(productId, quantity, variantId);

            return Json(new
            {
                success = true,
                message = "Added to cart",
                totalItems = _cart.GetTotalItems()
            });
        }

        // POST: /Cart/Update
        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            _cart.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId)
        {
            _cart.RemoveItem(productId);
            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            _cart.Clear();
            return RedirectToAction("Index");
        }

        // GET: /Cart/Count — navbar badge ke liye
        [HttpGet]
        public IActionResult Count()
        {
            return Json(new { count = _cart.GetTotalItems() });
        }
    }
}