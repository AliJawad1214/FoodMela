using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodMela.Models
{
    public enum ProductStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        // Relationship with Vendor (ApplicationUser)
        public string VendorId { get; set; }   // FK → AspNetUsers table

        public ApplicationUser Vendor { get; set; }   // Navigation property

        public ProductStatus Status { get; set; } = ProductStatus.Pending; // default pending
    }
}
