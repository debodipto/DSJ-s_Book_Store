using DSJsBookStore.Models;

namespace DSJsBookStore.Models
{
    public class AnalyticsDashboardViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBooks { get; set; }

        public IEnumerable<dynamic> MonthlySales { get; set; } = new List<dynamic>();
        public IEnumerable<dynamic> TopSellingBooks { get; set; } = new List<dynamic>();
        public IEnumerable<Order> RecentOrders { get; set; } = new List<Order>();
        public IEnumerable<Stock> LowStockBooks { get; set; } = new List<Stock>();
    }
}