using FoodMela.Data;
using FoodMela.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodMela.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View Cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");


            // check if customer already has an active order
            var hasActiveOrder = await _context.Orders
                .AnyAsync(o => o.UserId == user.Id &&
                               (o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProcess));

            ViewBag.HasActiveOrder = hasActiveOrder;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Vendor) // 👈 include vendor here
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            return View(cart);
        }

        // Add to Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Optional but recommended: ensure product exists (no status check)
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Find or create cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            // Add or increment
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem { ProductId = productId, Quantity = 1, Cart = cart };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // Remove from Cart
        // Remove one quantity
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--; // just decrease
                    _context.CartItems.Update(cartItem);
                }
                else
                {
                    _context.CartItems.Remove(cartItem); // remove if last one
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Remove all quantities of a product
        public async Task<IActionResult> RemoveAll(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem); // remove entirely
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


    }
}
