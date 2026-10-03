using JewelleryCommerce.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelleryCommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Categories (3)
            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Featured products (top 8 by rating)
            var featured = await _db.Products
                .Include(p => p.Vendor)
                .Include(p => p.Images)
                .Where(p => p.IsApproved && p.IsActive)
                .OrderByDescending(p => p.Rating)
                .Take(8)
                .ToListAsync();

            return View(featured);
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}