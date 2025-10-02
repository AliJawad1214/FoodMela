using Microsoft.AspNetCore.Identity;

namespace FoodMela.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; }
        // Roles will be handled by Identity, no need for manual Role property
    }
}
