using FoodMela.Data;
using FoodMela.Hubs;
using FoodMela.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace FoodMela.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // ✅ get cart by userId and include items + product
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Home");

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var checkoutVM = new CheckoutViewModel
            {
                CartItems = cart.CartItems,
                CustomerName = profile?.FullName ?? user.UserName,
                CustomerAddress = profile?.Address,
                CustomerPhone = profile?.PhoneNumber
            };

            return View(checkoutVM);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Vendor)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Home");

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                CustomerName = profile?.FullName ?? user.UserName,
                CustomerAddress = profile?.Address,
                CustomerPhone = profile?.PhoneNumber,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            // 🔔 Notify Vendor (real-time)
            var vendorIds = order.OrderItems
                .Select(oi => oi.Product.VendorId)
                .Distinct()
                .ToList();

            foreach (var vendorId in vendorIds)
            {
                System.Diagnostics.Debug.WriteLine($"🔔 Sending notification to VendorId: {vendorId}");
                var notification = new Notification
                {
                    UserId = vendorId,
                    Message = $"📦 New order received from {profile?.FullName ?? user.UserName}"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(vendorId)
                    .SendAsync("ReceiveNotification",
                        $"📦 New order received from {order.CustomerName}");
            }

            return RedirectToAction("Confirmation", new { id = order.Id });

        }

        // GET: /Order/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Vendor)  // 👈 Include Vendor
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }

    }
}
