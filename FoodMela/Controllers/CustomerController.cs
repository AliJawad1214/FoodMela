using FoodMela.Data;
using FoodMela.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodMela.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (profile == null)
            {
                profile = new CustomerProfile { UserId = user.Id, FullName = user.FullName };
            }

            return View(profile);
        }

        // POST: Customer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (existingProfile == null)
                {
                    model.UserId = user.Id;
                    _context.Add(model);
                }
                else
                {
                    existingProfile.FullName = model.FullName;
                    existingProfile.Address = model.Address;
                    existingProfile.City = model.City;
                    existingProfile.PhoneNumber = model.PhoneNumber;
                    _context.Update(existingProfile);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProfileView));
            }

            return View(model);
        }

        // GET: Customer/ProfileView
        public async Task<IActionResult> ProfileView()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (profile == null) return RedirectToAction(nameof(Profile));

            return View(profile);
        }
    }
}
