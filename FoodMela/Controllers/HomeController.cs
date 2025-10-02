using FoodMela.Data;
using FoodMela.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodMela.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Vendor)   // 👈 Include vendor data
                .Where(p => p.Status == ProductStatus.Approved)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Vendor)   // 👈 Include vendor data
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == ProductStatus.Approved);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> ByVendor(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
                return NotFound();

            var vendor = await _context.Users.FindAsync(vendorId);
            if (vendor == null) return NotFound();

            var products = await _context.Products
                .Include(p => p.Vendor)
                .Where(p => p.VendorId == vendorId)
                .ToListAsync();

            ViewBag.VendorName = vendor.FullName;

            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
