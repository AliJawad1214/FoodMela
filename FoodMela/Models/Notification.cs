namespace FoodMela.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }   // recipient
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }

}
