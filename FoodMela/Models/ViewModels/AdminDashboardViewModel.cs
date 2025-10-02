namespace FoodMela.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalVendors { get; set; }
        public int TotalCustomers { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<VendorSalesDto> TopVendors { get; set; } = new();
        public List<ProductSalesDto> TopProducts { get; set; } = new();
        public List<SalesOverTimeDto> SalesOverTime { get; set; } = new();
    }

    public class VendorSalesDto
    {
        public string VendorName { get; set; }
        public decimal Revenue { get; set; }
    }
}
