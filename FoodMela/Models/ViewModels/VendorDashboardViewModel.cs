namespace FoodMela.Models.ViewModels
{
    public class VendorDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<ProductSalesDto> TopProducts { get; set; } = new();
        public List<SalesOverTimeDto> SalesOverTime { get; set; } = new();
    }

    public class ProductSalesDto
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesOverTimeDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
