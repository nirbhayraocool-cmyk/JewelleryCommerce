using JewelleryCommerce.Web.Data;
using Microsoft.EntityFrameworkCore;
using JewelleryCommerce.Web.Models.Entities;
using JewelleryCommerce.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JewelleryCommerce.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _db;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,AppDbContext Db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = Db;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Role-based redirect
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return Redirect("/Admin/Dashboard");
                if (await _userManager.IsInRoleAsync(user, "Vendor"))
                    return Redirect("/Vendor/Dashboard");

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "Email already registered");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        // GET: /Account/Orders
        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var orders = await _db.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var vm = orders.Select(o => new OrderListViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                PaymentMode = o.PaymentMode.ToString(),
                ItemCount = o.Items.Count,
                FirstImageUrl = o.Items.FirstOrDefault()?.Product.Images
                                    .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                ?? o.Items.FirstOrDefault()?.Product.Images.FirstOrDefault()?.ImageUrl
                                ?? "https://via.placeholder.com/200"
            }).ToList();

            return View(vm);
        }

        // GET: /Account/OrderDetails/5
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Vendor)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            var vm = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                PaymentMode = order.PaymentMode.ToString(),
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                ShippingAddress = order.ShippingAddress,
                Pincode = order.Pincode,
                Items = order.Items.Select(i => new OrderItemVM
                {
                    ProductId = i.ProductId,
                    Title = i.Product.Title,
                    Slug = i.Product.Slug,
                    ImageUrl = i.Product.Images.FirstOrDefault(x => x.IsPrimary)?.ImageUrl
                               ?? i.Product.Images.FirstOrDefault()?.ImageUrl
                               ?? "https://via.placeholder.com/200",
                    VendorName = i.Vendor.ShopName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Status = i.Status.ToString()
                }).ToList()
            };

            return View(vm);
        }
        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}