using System.ComponentModel.DataAnnotations;

namespace FoodMela.Models
{
    public class CustomerProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; }   // FK to ApplicationUser
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }
}
