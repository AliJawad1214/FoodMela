using FoodMela.Data;
using FoodMela.Models;
using FoodMela.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> ManageProducts()
    {
        var products = await _context.Products.Include(p => p.Vendor).ToListAsync();
        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Status = ProductStatus.Approved;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageProducts));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.Status = ProductStatus.Rejected;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ManageProducts));
    }

    public async Task<IActionResult> Dashboard()
    {
        var users = _userManager.Users.ToList();
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Vendor)
            .ToListAsync();

        var vendors = await _userManager.GetUsersInRoleAsync("Vendor");
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var model = new AdminDashboardViewModel
        {
            TotalUsers = users.Count,
            TotalVendors = vendors.Count,
            TotalCustomers = customers.Count,

            TotalOrders = orders.Count,
            TotalRevenue = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)),

            OrdersByStatus = orders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),

            TopVendors = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.Vendor.FullName)
                .Select(g => new VendorSalesDto
                {
                    VendorName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.Product.Price)
                })
                .OrderByDescending(v => v.Revenue)
                .Take(5)
                .ToList(),

            TopProducts = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
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
                    Revenue = g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price))
                })
                .OrderBy(s => s.Date)
                .ToList()
        };

        return View(model);
    }
}
