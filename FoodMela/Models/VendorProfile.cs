using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodMela.Models
{
    public class VendorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ShopName { get; set; }

        public string? ShopAddress { get; set; }

        public string? ContactNumber { get; set; }

        public string? Description { get; set; }

        public string? LogoPath { get; set; } // optional, for logo image path

        // Relationship with ApplicationUser
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
