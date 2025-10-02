using FoodMela.Data;
using FoodMela.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodMela.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: VendorProducts
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var products = await _context.Products
                .Where(p => p.VendorId == user.Id)
                .ToListAsync();

            return View(products);
        }

        // GET: VendorProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VendorProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("VendorId");
            ModelState.Remove("Vendor");

            if (ModelState.IsValid)
            {
                product.VendorId = user.Id;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Validation error: " + err.ErrorMessage);
                }
            }
            return View(product);
        }

        // GET: VendorProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == user.Id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: VendorProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("VendorId");
            ModelState.Remove("Vendor");

            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.VendorId = user.Id; // ensure ownership
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == id && e.VendorId == user.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: VendorProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id && m.VendorId == user.Id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: VendorProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == user.Id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
