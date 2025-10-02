using FoodMela.Data;
using FoodMela.Hubs;
using FoodMela.Models;
using FoodMela.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FoodMela.Controllers
{
    [Authorize(Roles = "Vendor")]   // only vendors can access
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public VendorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: Vendor/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (profile == null)
            {
                profile = new VendorProfile { UserId = user.Id }; // new profile if not exists
            }

            return View(profile);
        }

        // POST: Vendor/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(VendorProfile model)
        {
            System.Diagnostics.Debug.WriteLine(">>> POST CALLED <<<");
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.VendorProfiles
                    .FirstOrDefaultAsync(v => v.UserId == user.Id);

                if (existingProfile == null)
                {
                    model.UserId = user.Id;
                    _context.Add(model);
                }
                else
                {
                    existingProfile.ShopName = model.ShopName;
                    existingProfile.ShopAddress = model.ShopAddress;
                    existingProfile.ContactNumber = model.ContactNumber;
                    existingProfile.Description = model.Description;
                    existingProfile.LogoPath = model.LogoPath;
                    // no need to call _context.Update(existingProfile);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Profile saved successfully!";
                return RedirectToAction(nameof(Details)); // reload page
            }
            else
            {
                foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Validation error: " + err.ErrorMessage);
                }
            }

            return View(model);
        }

        // GET: Vendor/Details
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (profile == null)
            {
                // No profile yet → redirect to create/edit form
                return RedirectToAction(nameof(Profile));
            }

            return View(profile); // show details page
        }

        public async Task<IActionResult> Orders()
        {
            var vendorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderItems.Any(oi => oi.Product.VendorId == vendorId))
                .ToListAsync();

            return View(orders);
        }

        // Accept Order
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.InProcess; // change status
            await _context.SaveChangesAsync();

            // Save notification in DB
            var notif = new Notification
            {
                UserId = order.UserId,
                Message = $"Your order #{order.Id} has been accepted!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Real-time SignalR push
            await _hubContext.Clients.User(order.UserId)
                .SendAsync("ReceiveNotification", notif.Message);

            return RedirectToAction(nameof(Orders));
        }

        // Mark Delivered
        [HttpPost]
        public async Task<IActionResult> Deliver(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = OrderStatus.Delivered; // change status
            await _context.SaveChangesAsync();

            var notif = new Notification
            {
                UserId = order.UserId,
                Message = $"Your order #{order.Id} has been delivered!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Real-time SignalR push
            await _hubContext.Clients.User(order.UserId)
                .SendAsync("ReceiveNotification", notif.Message);

            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> Dashboard()
        {
            var vendor = await _userManager.GetUserAsync(User);
            var vendorId = vendor.Id;

            // Get all orders that include this vendor's products
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.VendorId == vendorId))
                .ToListAsync();

            var model = new VendorDashboardViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .Sum(o => o.OrderItems
                        .Where(oi => oi.Product.VendorId == vendorId)
                        .Sum(oi => oi.Quantity * oi.Product.Price)),

                OrdersByStatus = orders
                    .GroupBy(o => o.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),

                TopProducts = orders
                    .SelectMany(o => o.OrderItems)
                    .Where(oi => oi.Product.VendorId == vendorId)
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new ProductSalesDto
                    {
                        ProductName = g.Key,
                        QuantitySold = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.Quantity * x.Product.Price)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(5)
                    .ToList(),

                SalesOverTime = orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .GroupBy(o => o.OrderDate)
                    .Select(g => new SalesOverTimeDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.OrderItems
                            .Where(oi => oi.Product.VendorId == vendorId)
                            .Sum(oi => oi.Quantity * oi.Product.Price))
                    })
                    .OrderBy(s => s.Date)
                    .ToList()
            };

            return View(model);
        }


    }
}
