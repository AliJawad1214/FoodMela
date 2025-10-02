namespace FoodMela.Models
{
    public enum TestStatus
    {
        Pending,
        Accepted,
        InProcess,
        Delivered
    }
    public class Test
    {
        public int Id { get; set; }
        public TestStatus Status { get; set; } = TestStatus.Pending;
    }
}
